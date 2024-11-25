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

namespace MealSync.Application.UseCases.WithdrawalRequests.Commands.UpdateWithdrawalRequestStatus;

public class UpdateWithdrawalRequestStatusHandler : ICommandHandler<UpdateWithdrawalRequestStatusCommand, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateWithdrawalRequestStatusHandler> _logger;

    public UpdateWithdrawalRequestStatusHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IWithdrawalRequestRepository withdrawalRequestRepository, IWalletRepository walletRepository,
        IWalletTransactionRepository walletTransactionRepository, ISystemResourceRepository systemResourceRepository,
        IUnitOfWork unitOfWork, ILogger<UpdateWithdrawalRequestStatusHandler> logger)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _systemResourceRepository = systemResourceRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(UpdateWithdrawalRequestStatusCommand request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var withdrawalRequest = await _withdrawalRequestRepository.GetForManageIncludeWalletAndShop(dormitoryIds, request.Id).ConfigureAwait(false);

        if (withdrawalRequest == default)
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }
        else if (!request.IsConfirm && request.Status == WithdrawalRequestStatus.Approved && (withdrawalRequest.Wallet.Shop!.Status == ShopStatus.Banning || withdrawalRequest.Wallet.Shop.Status == ShopStatus.Banned))
        {
            return Result.Warning(new
            {
                Code = MessageCode.W_MODERATOR_SHOP_BAN_STILL_WITHDRAWAL.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_MODERATOR_SHOP_BAN_STILL_WITHDRAWAL.GetDescription()),
            });
        }
        else
        {
            if (!(withdrawalRequest.Status == WithdrawalRequestStatus.Pending && request.Status == WithdrawalRequestStatus.UnderReview)
                && !(withdrawalRequest.Status == WithdrawalRequestStatus.UnderReview && (request.Status == WithdrawalRequestStatus.Approved || request.Status == WithdrawalRequestStatus.Rejected)))
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
            }
            else
            {
                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                    if (withdrawalRequest.Status == WithdrawalRequestStatus.Pending && request.Status == WithdrawalRequestStatus.UnderReview)
                    {
                        withdrawalRequest.Status = WithdrawalRequestStatus.UnderReview;

                        _withdrawalRequestRepository.Update(withdrawalRequest);
                    }
                    else if (withdrawalRequest.Status == WithdrawalRequestStatus.UnderReview && (request.Status == WithdrawalRequestStatus.Approved || request.Status == WithdrawalRequestStatus.Rejected))
                    {
                        if (request.Status == WithdrawalRequestStatus.Approved)
                        {
                            var transactionWithdrawalAvailableAmountOfShop = new WalletTransaction()
                            {
                                WalletFromId = withdrawalRequest.WalletId,
                                AvaiableAmountBefore = withdrawalRequest.Wallet.AvailableAmount,
                                IncomingAmountBefore = withdrawalRequest.Wallet.IncomingAmount,
                                ReportingAmountBefore = withdrawalRequest.Wallet.ReportingAmount,
                                Amount = -withdrawalRequest.Amount,
                                Type = WalletTransactionType.Withdrawal,
                                Description = $"Rút tiền từ tiền có sẵn {MoneyUtils.FormatMoneyWithDots(withdrawalRequest.Amount)} VNĐ để chuyển tiền cho cửa hàng",
                            };

                            withdrawalRequest.Wallet.AvailableAmount -= withdrawalRequest.Amount;
                            withdrawalRequest.Status = WithdrawalRequestStatus.Approved;

                            _withdrawalRequestRepository.Update(withdrawalRequest);
                            await _walletTransactionRepository.AddAsync(transactionWithdrawalAvailableAmountOfShop).ConfigureAwait(false);
                        }
                        else
                        {
                            withdrawalRequest.Status = WithdrawalRequestStatus.Rejected;
                            withdrawalRequest.Reason = request.Reason;

                            _withdrawalRequestRepository.Update(withdrawalRequest);
                        }
                    }

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                    return Result.Success(new
                    {
                        Code = MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription(),
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_MODERATOR_UPDATE_STATUS_SUCCESS.GetDescription()),
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