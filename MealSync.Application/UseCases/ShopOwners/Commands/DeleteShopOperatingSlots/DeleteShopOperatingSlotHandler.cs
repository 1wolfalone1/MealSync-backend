using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MealSync.Application.UseCases.ShopOwners.Commands.DeleteShopOperatingSlots;

public class DeleteShopOperatingSlotHandler : ICommandHandler<DeleteShopOperatingSlotCommand, Result>
{
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IFoodOperatingSlotRepository _foodOperatingSlotRepository;
    private readonly ILogger<DeleteShopOperatingSlotHandler> _logger;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly ICacheService _cacheService;

    private const string KEY_CACHE_CODE = "{0}-CodeConfirmDeleteOperatingSlot";

    public DeleteShopOperatingSlotHandler(IOperatingSlotRepository operatingSlotRepository, IUnitOfWork unitOfWork, ICurrentPrincipalService currentPrincipalService, IFoodOperatingSlotRepository foodOperatingSlotRepository,
        ILogger<DeleteShopOperatingSlotHandler> logger, ICacheService cacheService, ICurrentAccountService currentAccountService, ISystemResourceRepository systemResourceRepository, IFoodRepository foodRepository)
    {
        _operatingSlotRepository = operatingSlotRepository;
        _unitOfWork = unitOfWork;
        _currentPrincipalService = currentPrincipalService;
        _foodOperatingSlotRepository = foodOperatingSlotRepository;
        _logger = logger;
        _cacheService = cacheService;
        _currentAccountService = currentAccountService;
        _systemResourceRepository = systemResourceRepository;
        _foodRepository = foodRepository;
    }

    public async Task<Result<Result>> Handle(DeleteShopOperatingSlotCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);

        if (string.IsNullOrEmpty(request.CodeConfirm))
        {
            var listProduct = _foodOperatingSlotRepository.Get(op => op.OperatingSlotId == request.Id).ToList();
            if (listProduct != null && listProduct.Count > 0)
            {
                var operatingSlot = _operatingSlotRepository.GetById(request.Id);
                var message = _systemResourceRepository.GetByResourceCode(MessageCode.W_OPERATING_SLOT_DELETE_INCLUDE_PRODUCT.GetDescription(), new object[]
                {
                    TimeFrameUtils.GetTimeHoursFormat(operatingSlot.StartTime),
                    TimeFrameUtils.GetTimeHoursFormat(operatingSlot.EndTime),
                    listProduct.Count,
                });

                // Generate code an set to cache
                var code = new Random().Next(1000, 10000).ToString();
                // Set to cache
                await _cacheService.SetCacheResponseAsync(string.Format(KEY_CACHE_CODE, _currentAccountService.GetCurrentAccount().Email), code, TimeSpan.FromSeconds(RedisConstant.TIME_ACCEPT_CHANGE_OPERATING_SLOT))
                    .ConfigureAwait(false);

                return Result.Warning(new
                {
                    Message = message,
                    CodeConfirm = code,
                });
            }
        }

        var listFoodOperating = _foodOperatingSlotRepository.GetOperatingSlotsWithFoodByOpId(request.Id);
        if (listFoodOperating != null && listFoodOperating.Count > 0)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                // Inactive all product
                var foods = listFoodOperating.Select(fos => fos.Food).ToList();
                foreach (var product in foods)
                {
                    product.Status = FoodStatus.UnActive;
                }

                _foodRepository.UpdateRange(foods);

                // Remove food operating slot
                _foodOperatingSlotRepository.RemoveRange(listFoodOperating);
                await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

                // Remove operating slot
                var operatingSlot = _operatingSlotRepository.GetById(request.Id);
                _operatingSlotRepository.Remove(operatingSlot);

                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                await _cacheService.RemoveCacheResponseAsync(string.Format(KEY_CACHE_CODE, _currentAccountService.GetCurrentAccount().Email)).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }
        }
        else
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                // Remove operating slot
                var operatingSlot = _operatingSlotRepository.GetById(request.Id);
                _operatingSlotRepository.Remove(operatingSlot);

                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        return Result.Success(new
        {
            Code = MessageCode.I_OPERATING_SLOT_DELETE_SUCCESS.GetDescription(),
            Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_OPERATING_SLOT_DELETE_SUCCESS.GetDescription()),
        });

    }

    private async Task ValidateAsync(DeleteShopOperatingSlotCommand request)
    {
        var operatingSlot = _operatingSlotRepository.Get(op => op.Id == request.Id
                                                               && op.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault();
        if (operatingSlot == default)
            throw new InvalidBusinessException(MessageCode.E_OPERATING_SLOT_NOT_FOUND.GetDescription(), HttpStatusCode.NotFound);

        if (!string.IsNullOrEmpty(request.CodeConfirm))
        {
            var getCode = await _cacheService.GetCachedResponseAsync(string.Format(KEY_CACHE_CODE, _currentAccountService.GetCurrentAccount().Email)).ConfigureAwait(false);
            var requestCodeConfirm = JsonConvert.DeserializeObject<string>(getCode ?? string.Empty);
            if (requestCodeConfirm != request.CodeConfirm)
            {
                throw new InvalidBusinessException(MessageCode.E_OPERATING_SLOT_CODE_CONFIRM_NOT_CORRECT.GetDescription(), HttpStatusCode.Conflict);
            }
        }
    }
}