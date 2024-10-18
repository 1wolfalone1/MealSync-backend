using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.ShopCategories.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Update;

public class UpdateShopCategoryHandler : ICommandHandler<UpdateShopCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateShopCategoryHandler> _logger;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopCategoryRepository _shopCategoryRepository;

    public UpdateShopCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdateShopCategoryHandler> logger, ICurrentPrincipalService currentPrincipalService, IShopCategoryRepository shopCategoryRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentPrincipalService = currentPrincipalService;
        _shopCategoryRepository = shopCategoryRepository;
    }

    public async Task<Result<Result>> Handle(UpdateShopCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shopCategory = _shopCategoryRepository.GetByIdAndShopId(request.Id, _currentPrincipalService.CurrentPrincipalId.Value);
            shopCategory.Name = request.Name;
            shopCategory.Description = request.Description;
            shopCategory.ImageUrl = request.ImageUrl;
            _shopCategoryRepository.Update(shopCategory);
            var response = _mapper.Map<ShopCategoryResponse>(shopCategory);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            return Result.Success(response);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(UpdateShopCategoryCommand request)
    {
        if (_shopCategoryRepository.GetByIdAndShopId(request.Id, _currentPrincipalService.CurrentPrincipalId.Value) == default)
            throw new InvalidBusinessException(MessageCode.E_SHOP_CATEGORY_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);

        if (_shopCategoryRepository.CheckExistName(request.Name, request.Id))
            throw new InvalidBusinessException(MessageCode.E_SHOP_CATEGORY_DOUBLE_NAME.GetDescription(), new object[]{ request.Name }, HttpStatusCode.Conflict);
    }
}