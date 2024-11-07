using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Wallets.Commands.Shop.CancelWithdrawalRequest;

public class CancelWithdrawalRequestHandler : ICommandHandler<CancelWithdrawalRequestCommand, Result>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelWithdrawalRequestHandler> _logger;

    public CancelWithdrawalRequestHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository, IShopRepository shopRepository,
        IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository,
        ISystemResourceRepository systemResourceRepository, ICurrentPrincipalService currentPrincipalService,
        IUnitOfWork unitOfWork, INotificationFactory notificationFactory, INotifierService notifierService,
        IMapper mapper, ILogger<CancelWithdrawalRequestHandler> logger)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _shopRepository = shopRepository;
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _systemResourceRepository = systemResourceRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Result>> Handle(CancelWithdrawalRequestCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = await _shopRepository.GetByAccountId(shopId).ConfigureAwait(false);

        var withdrawalRequest = await _withdrawalRequestRepository.GetByIdAndWalletId(request.Id, shop.WalletId).ConfigureAwait(false);

        if (withdrawalRequest == default)
        {
            throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            if (withdrawalRequest.Status != WithdrawalRequestStatus.Pending)
            {
                throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_CAN_NOT_CANCEL.GetDescription(), new object[] { request.Id });
            }
            else if (!request.IsConfirm)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_WITHDRAWAL_REQUEST_CANCEL.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_WITHDRAWAL_REQUEST_CANCEL.GetDescription(), new object[] { request.Id }),
                });
            }
            else
            {
                var wallet = _walletRepository.GetById(shop.WalletId)!;

                withdrawalRequest.Status = WithdrawalRequestStatus.Cancelled;

                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                    _withdrawalRequestRepository.Update(withdrawalRequest);

                    // Commit transaction
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                    return Result.Success(new
                    {
                        Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_WITHDRAWAL_CANCEL_SUCCESS.GetDescription(), new object[] { request.Id }),
                        WalletInfo = _mapper.Map<WalletSummaryResponse>(wallet),
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