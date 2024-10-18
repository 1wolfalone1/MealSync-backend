using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Create;

public class CreateShopCategoryHandler : ICommandHandler<CreateShopCategoryCommand, Result>
{
    private readonly ILogger<CreateShopCategoryHandler> _logger;
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public CreateShopCategoryHandler(
        ILogger<CreateShopCategoryHandler> logger, IShopCategoryRepository shopCategoryRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper, IUnitOfWork unitOfWork
    )
    {
        _logger = logger;
        _shopCategoryRepository = shopCategoryRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Result>> Handle(CreateShopCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        var accountId = _currentPrincipalService.CurrentPrincipalId!;
        var lastedShopCategory = _shopCategoryRepository.GetLastedByShopId(accountId.Value);
        ShopCategory shopCategory = new ShopCategory
        {
            ShopId = accountId.Value,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
        };
        if (lastedShopCategory == null)
        {
            shopCategory.DisplayOrder = 1;
        }
        else
        {
            shopCategory.DisplayOrder = lastedShopCategory.DisplayOrder + 1;
        }

        // Save new shop category
        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            await _shopCategoryRepository.AddAsync(shopCategory).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Create(_mapper.Map<ShopCategoryResponse>(_shopCategoryRepository.GetById(shopCategory.Id)));
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }

    private void Validate(CreateShopCategoryCommand request)
    {
        if (_shopCategoryRepository.CheckExistName(request.Name))
            throw new InvalidBusinessException(MessageCode.E_SHOP_CATEGORY_DOUBLE_NAME.GetDescription(), new object[]{request.Name}, HttpStatusCode.Conflict);
    }
}