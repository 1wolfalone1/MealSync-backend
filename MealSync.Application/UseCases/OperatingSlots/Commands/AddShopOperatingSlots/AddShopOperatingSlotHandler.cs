using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.OperatingSlots.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.OperatingSlots.Commands.AddShopOperatingSlots;

public class AddShopOperatingSlotHandler : ICommandHandler<AddShopOperatingSlotCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ILogger<AddShopOperatingSlotHandler> _logger;
    private readonly IMapper _mapper;

    public AddShopOperatingSlotHandler(IUnitOfWork unitOfWork, IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService, IOperatingSlotRepository operatingSlotRepository, ILogger<AddShopOperatingSlotHandler> logger, ISystemResourceRepository systemResourceRepository, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _operatingSlotRepository = operatingSlotRepository;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(AddShopOperatingSlotCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var operatingSlot = new OperatingSlot()
            {
                Title = request.Title,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                ShopId = _currentPrincipalService.CurrentPrincipalId.Value,
            };
            await _operatingSlotRepository.AddAsync(operatingSlot).ConfigureAwait(false);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            var response = _mapper.Map<OperatingSlotResponse>(operatingSlot);
            return Result.Success(response);
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(AddShopOperatingSlotCommand request)
    {
        var listOperatingSlot = _operatingSlotRepository.Get(x => x.ShopId == _currentPrincipalService.CurrentPrincipalId).ToList();
        if (listOperatingSlot != null && listOperatingSlot.Count > 1)
        {
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