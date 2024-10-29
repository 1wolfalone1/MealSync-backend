using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Promotions.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Promotions.Commands.UpdateInfo;

public class UpdatePromotionInfoHandler : ICommandHandler<UpdatePromotionInfoCommand, Result>
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePromotionInfoHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public UpdatePromotionInfoHandler(
        IPromotionRepository promotionRepository, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, IMapper mapper, ILogger<UpdatePromotionInfoHandler> logger, ISystemResourceRepository systemResourceRepository)
    {
        _promotionRepository = promotionRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(UpdatePromotionInfoCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var promotion = await _promotionRepository.GetByIdAndShopId(request.Id, shopId).ConfigureAwait(false);

        if (promotion == default || promotion.Status == PromotionStatus.Delete)
        {
            throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            if (request.UsageLimit < promotion.NumberOfUsed)
            {
                throw new InvalidBusinessException(
                    MessageCode.E_PROMOTION_MUST_BE_GREATER_OR_EQUAL_TO_USED_QUANTITY.GetDescription(),
                    new object[] { promotion.NumberOfUsed });
            }
            else
            {
                promotion.Title = request.Title;
                promotion.Description = request.Description;
                promotion.BannerUrl = request.BannerUrl;
                promotion.AmountRate = request.AmountRate;
                promotion.MaximumApplyValue = request.MaximumApplyValue;
                promotion.AmountValue = request.AmountValue;
                promotion.MinOrdervalue = request.MinOrdervalue;
                promotion.StartDate = DateTime.SpecifyKind(request.StartDate, DateTimeKind.Utc);
                promotion.EndDate = DateTime.SpecifyKind(request.EndDate, DateTimeKind.Utc);
                promotion.UsageLimit = request.UsageLimit;
                promotion.ApplyType = request.ApplyType;
                promotion.Status = request.Status;

                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    _promotionRepository.Update(promotion);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                    return Result.Success(new
                    {
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_PROMOTION_UPDATE_INFO_SUCCESS.GetDescription()),
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
}