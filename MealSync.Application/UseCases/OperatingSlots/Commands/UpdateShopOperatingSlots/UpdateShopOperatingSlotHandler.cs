using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OperatingSlots.Commands.UpdateShopOperatingSlots;

public class UpdateShopOperatingSlotHandler : ICommandHandler<UpdateShopOperatingSlotCommand, Result>
{
    private readonly IShopRepository _shopRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFoodOperatingSlotRepository _foodOperatingSlotRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ICacheService _cacheService;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly ILogger<UpdateShopOperatingSlotHandler> _logger;

    public UpdateShopOperatingSlotHandler(IShopRepository shopRepository, IOperatingSlotRepository operatingSlotRepository, IUnitOfWork unitOfWork, ILogger<UpdateShopOperatingSlotHandler> logger,
        IFoodOperatingSlotRepository foodOperatingSlotRepository, ISystemResourceRepository systemResourceRepository, ICurrentPrincipalService currentPrincipalService, ICacheService cacheService,
        ICurrentAccountService currentAccountService)
    {
        _shopRepository = shopRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _foodOperatingSlotRepository = foodOperatingSlotRepository;
        _systemResourceRepository = systemResourceRepository;
        _currentPrincipalService = currentPrincipalService;
        _cacheService = cacheService;
        _currentAccountService = currentAccountService;
    }

    public async Task<Result<Result>> Handle(UpdateShopOperatingSlotCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);

        if (!request.IsConfirm)
        {
            var listProduct = _foodOperatingSlotRepository.Get(op => op.OperatingSlotId == request.Id).ToList();
            if (listProduct != null && listProduct.Count > 0)
            {
                var operatingSlot = _operatingSlotRepository.GetById(request.Id);
                var message = _systemResourceRepository.GetByResourceCode(MessageCode.W_OPERATING_SLOT_CHANGE_INCLUDE_PRODUCT.GetDescription(), new object[]
                {
                    TimeFrameUtils.GetTimeHoursFormat(operatingSlot.StartTime),
                    TimeFrameUtils.GetTimeHoursFormat(operatingSlot.EndTime),
                    listProduct.Count,
                });

                return Result.Warning(new
                {
                    Code = MessageCode.W_OPERATING_SLOT_CHANGE_INCLUDE_PRODUCT.GetDescription(),
                    Message = message,
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var operatingSlot = _operatingSlotRepository.GetById(request.Id);
            operatingSlot.Title = request.Title;
            operatingSlot.StartTime = request.StartTime;
            operatingSlot.EndTime = request.EndTime;
            _operatingSlotRepository.Update(operatingSlot);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(new
            {
                Code = MessageCode.I_OPERATING_SLOT_CHANGE_SUCCESS.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_OPERATING_SLOT_CHANGE_SUCCESS.GetDescription()),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task ValidateAsync(UpdateShopOperatingSlotCommand request)
    {
        var operatingSlot = _operatingSlotRepository.Get(op => op.Id == request.Id
                                                               && op.ShopId == _currentPrincipalService.CurrentPrincipalId).SingleOrDefault();
        if (operatingSlot == default)
            throw new InvalidBusinessException(MessageCode.E_OPERATING_SLOT_NOT_FOUND.GetDescription(), new object[] {request.Id}, HttpStatusCode.NotFound);

        var listOperatingSlot = _operatingSlotRepository.Get(x => x.ShopId == _currentPrincipalService.CurrentPrincipalId).ToList();
        if (listOperatingSlot != null && listOperatingSlot.Count > 1)
        {
            listOperatingSlot.Remove(listOperatingSlot.SingleOrDefault(x => x.Id == request.Id));
            listOperatingSlot.Add(new OperatingSlot()
            {
                StartTime = request.StartTime,
                EndTime = request.EndTime,
            });
            var listSlot = listOperatingSlot.Select(op => (op.StartTime, op.EndTime)).ToList();
            if (TimeUtils.HasOverlappingTimeSegment(listSlot))
            {
                throw new InvalidBusinessException(MessageCode.E_OPERATING_SLOT_OVERLAP.GetDescription(), new object[]
                {
                    TimeFrameUtils.GetTimeHoursFormat(request.StartTime),
                    TimeFrameUtils.GetTimeHoursFormat(request.EndTime),
                }, HttpStatusCode.Conflict);
            }
        }
    }
}