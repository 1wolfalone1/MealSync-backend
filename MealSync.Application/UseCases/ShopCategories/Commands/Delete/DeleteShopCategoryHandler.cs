using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopCategories.Commands.Delete;

public class DeleteShopCategoryHandler : ICommandHandler<DeleteShopCategoryCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopCategoryRepository _shopCategoryRepository;
    private readonly ILogger<DeleteShopCategoryHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public DeleteShopCategoryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentPrincipalService currentPrincipalService, IShopCategoryRepository shopCategoryRepository, ILogger<DeleteShopCategoryHandler> logger, ISystemResourceRepository systemResourceRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
        _shopCategoryRepository = shopCategoryRepository;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(DeleteShopCategoryCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shopCategory = _shopCategoryRepository.GetById(request.Id);
            _shopCategoryRepository.Remove(shopCategory);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(new
            {
                Code = MessageCode.I_SHOP_CATEGORY_DELETE_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_CATEGORY_DELETE_SUCCESS.GetDescription(), shopCategory.Name),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(DeleteShopCategoryCommand request)
    {
        var shopCategory = _shopCategoryRepository.Get(sc => sc.ShopId == _currentPrincipalService.CurrentPrincipalId.Value && sc.Id == request.Id)
            .Include(sc => sc.Foods).SingleOrDefault();
        if (shopCategory == default)
            throw new InvalidBusinessException(MessageCode.E_SHOP_CATEGORY_NOT_FOUND.GetDescription(), new object[] { request.Id }, HttpStatusCode.NotFound);

        if (shopCategory.Foods.Count > 0)
            throw new InvalidBusinessException(MessageCode.E_SHOP_CATEGORY_HAVE_FOOD_LINKED.GetDescription(), new object[] { shopCategory.Foods.Count }, HttpStatusCode.NotFound);
    }
}