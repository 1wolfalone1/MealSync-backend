using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Promotions.Commands.UpdateStatus;

public class UpdatePromotionStatusHandler : ICommandHandler<UpdatePromotionStatusCommand, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePromotionStatusHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public UpdatePromotionStatusHandler(
        IPromotionRepository promotionRepository, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdatePromotionStatusHandler> logger, ISystemResourceRepository systemResourceRepository)
    {
        _promotionRepository = promotionRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UpdatePromotionStatusCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var promotion = await _promotionRepository.GetByIdAndShopId(request.Id, shopId).ConfigureAwait(false);

        if (promotion == default || promotion.Status == PromotionStatus.Delete)
        {
            throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            try
            {
                // Begin transaction
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                promotion.Status = request.Status;
                _promotionRepository.Update(promotion);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                return Result.Success(new
                {
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_PROMOTION_UPDATE_STATUS_SUCCESS.GetDescription()),
                    PromotionInfo = _mapper.Map<PromotionDetailOfShop>(promotion),
                });
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
}