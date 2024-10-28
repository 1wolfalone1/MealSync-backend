using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Promotions.Commands.Create;

public class CreatePromotionHandler : ICommandHandler<CreatePromotionCommand, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePromotionHandler> _logger;

    public CreatePromotionHandler(
        IPromotionRepository promotionRepository, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreatePromotionHandler> logger)
    {
        _promotionRepository = promotionRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var promotion = new Promotion
        {
            ShopId = shopId,
            Title = request.Title,
            Description = request.Description,
            BannerUrl = request.BannerUrl,
            Type = PromotionTypes.ShopPromotion,
            AmountRate = request.AmountRate,
            MaximumApplyValue = request.MaximumApplyValue,
            AmountValue = request.AmountValue,
            MinOrdervalue = request.MinOrdervalue,
            StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc),
            UsageLimit = request.UsageLimit,
            NumberOfUsed = 0,
            ApplyType = request.ApplyType,
            Status = request.Status,
        };

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _promotionRepository.AddAsync(promotion).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Create(_mapper.Map<PromotionDetailOfShop>(promotion));
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }
}