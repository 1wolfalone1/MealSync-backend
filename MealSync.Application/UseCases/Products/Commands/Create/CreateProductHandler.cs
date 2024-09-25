using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Products.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Products.Commands.Create;

public class CreateProductHandler : ICommandHandler<CreateProductCommand, Result>
{
    private readonly ILogger<CreateProductHandler> _logger;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IShopOwnerRepository _shopOwnerRepository;
    private readonly IOperatingDayRepository _operatingDayRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductHandler(
        ILogger<CreateProductHandler> logger, ICategoryRepository categoryRepository,
        IProductRepository productRepository, IShopOwnerRepository shopOwnerRepository,
        IOperatingDayRepository operatingDayRepository, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, IMapper mapper)
    {
        _logger = logger;
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _shopOwnerRepository = shopOwnerRepository;
        _operatingDayRepository = operatingDayRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var accountId = _currentPrincipalService.CurrentPrincipalId!;

        await ValidateBusinessRequest(request, accountId).ConfigureAwait(false);

        // Create new product
        var shop = await _shopOwnerRepository.GetByAccountId(accountId.Value).ConfigureAwait(false);

        var product = new Product
        {
            ShopOwnerId = shop.Id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImgUrl,
            TotalOrder = 0,
            IsSoldOut = false,
            Status = ProductStatus.Active,
        };

        // Create product operating hours
        List<ProductOperatingHour> productOperatingHours = new List<ProductOperatingHour>();
        request.OperatingHours.ForEach(operatingHour =>
        {
            productOperatingHours.Add(new ProductOperatingHour
            {
                OperatingDayId = operatingHour.OperatingDayId,
                StartTime = operatingHour.StartTime,
                EndTime = operatingHour.EndTime,
            });
        });

        // Create product category list
        List<ProductCategory> productCategories = new List<ProductCategory>();
        request.CategoryIds.ForEach(id =>
        {
            productCategories.Add(new ProductCategory
            {
                CategoryId = id,
            });
        });

        // Create question list if present
        var questions = new List<ProductQuestion>();
        if (request.Questions != null && request.Questions.Count > 0)
        {
            request.Questions.ForEach(requestQuestion =>
            {
                var options = new List<ProductQuestionOption>();
                requestQuestion.Options.ForEach(requestOption =>
                {
                    var option = new ProductQuestionOption
                    {
                        Description = requestOption.Description,
                        IsPricing = requestOption.IsPricing,
                        Price = requestOption.Price,
                        ImageUrl = requestOption.ImageUrl,
                        Status = ToppingOptionStatus.Active,
                    };
                    options.Add(option);
                });
                var question = new ProductQuestion
                {
                    Type = requestQuestion.Type,
                    Description = requestQuestion.Description,
                    Status = QuestionStatus.Active,
                    ProductQuestionOptions = options,
                };
                questions.Add(question);
            });
        }

        // Assign to product
        product.ProductOperatingHours = productOperatingHours;
        product.ProductCategories = productCategories;
        product.ProductQuestions = questions;

        // Save product
        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _productRepository.AddAsync(product).ConfigureAwait(false);

            // Update total product of shop
            shop.TotalProduct += 1;
            _shopOwnerRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Create(_mapper.Map<ProductDetailResponse>(_productRepository.GetByIdIncludeAllInfo(product.Id)));
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }

    private async Task ValidateBusinessRequest(CreateProductCommand request, long? accountId)
    {

        // Check existed category
        var existedCategory = await _categoryRepository.CheckExistedByIds(request.CategoryIds).ConfigureAwait(false);

        if (!existedCategory)
        {
            throw new InvalidBusinessException(MessageCode.E_CATEGORY_NOT_FOUND.GetDescription());
        }

        // Check existed operating day
        var groupedByDate = request.OperatingHours.GroupBy(x => x.OperatingDayId);

        foreach (var group in groupedByDate)
        {
            long operatingDayId = group.Key;
            var operatingDay = _operatingDayRepository.GetByIdAndShopId(operatingDayId, accountId.Value);

            if (operatingDay == null)
            {
                throw new InvalidBusinessException(
                    MessageCode.E_OPERATING_DAY_NOT_FOUND.GetDescription(),
                    [operatingDayId]
                );
            }
            else
            {
                var timeSegmentList = group.Select(x => (x.StartTime, x.EndTime)).ToList();

                if (timeSegmentList.Count != 1 && TimeUtils.HasOverlappingTimeSegment(timeSegmentList))
                {
                    throw new InvalidBusinessException(
                        MessageCode.E_OPERATING_FRAME_HAS_OVERLAPPING.GetDescription(),
                        [operatingDayId]
                    );
                }
                timeSegmentList.ForEach(timeSegment =>
                {
                    var timeSegments = TimeUtils.ConvertToTimeSegment(timeSegment.StartTime, timeSegment.EndTime);
                    var operatingFrameActiveList = operatingDay.OperatingFrames.Where(frame => frame.IsActive).ToList();
                    bool isAllSegmentsActive = timeSegments.All(segment =>
                    {
                        return operatingFrameActiveList.Any(frame => frame.StartTime == segment.SegmentStart && frame.EndTime == segment.SegmentEnd);
                    });

                    if (!isAllSegmentsActive)
                    {
                        throw new InvalidBusinessException(
                            MessageCode.E_OPERATING_FRAME_HAS_NOT_ACTIVE_TIME.GetDescription(),
                            [operatingDayId]
                        );
                    }
                });
            }
        }
    }
}