using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateShopStatus;

public class UpdateShopStatusInactiveActiveHandler : ICommandHandler<UpdateShopStatusInactiveActiveCommand, Result>
{
    private readonly ILogger<UpdateShopStatusInactiveActiveHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly INotifierService _notifierService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IAccountFlagRepository _accountFlagRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IEmailService _emailService;
    private readonly ISystemConfigRepository _systemConfigRepository;

    public UpdateShopStatusInactiveActiveHandler(ILogger<UpdateShopStatusInactiveActiveHandler> logger, IUnitOfWork unitOfWork, IShopRepository shopRepository, ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository,
        ISystemResourceRepository systemResourceRepository, INotifierService notifierService, INotificationFactory notificationFactory, IEmailService emailService, IAccountFlagRepository accountFlagRepository, ICurrentAccountService currentAccountService, IAccountRepository accountRepository, ISystemConfigRepository systemConfigRepository)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
        _systemResourceRepository = systemResourceRepository;
        _notifierService = notifierService;
        _notificationFactory = notificationFactory;
        _emailService = emailService;
        _accountFlagRepository = accountFlagRepository;
        _currentAccountService = currentAccountService;
        _accountRepository = accountRepository;
        _systemConfigRepository = systemConfigRepository;
    }

    public async Task<Result<Result>> Handle(UpdateShopStatusInactiveActiveCommand request, CancellationToken cancellationToken)
    {
        // Validate
        await ValidateAsync(request).ConfigureAwait(false);

        // Change to InActive
        if (request.Status == (int)ShopStatus.InActive)
        {
            if (!request.IsConfirm)
            {
                var listOrderProcessing = _orderRepository.Get(o => OrderConstant.LIST_ORDER_STATUS_IN_PROCESSING.Any(x => x == o.Status)).ToList();
                if (listOrderProcessing != default && listOrderProcessing.Count > 1)
                {
                    var numOfPendingOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Pending).Count();
                    var numOfConfirmOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Confirmed).Count();
                    var numOfPreparingOrder = listOrderProcessing.Where(o => o.Status == OrderStatus.Preparing).Count();
                    return Result.Warning(new
                    {
                        Code = MessageCode.W_SHOP_HAVE_ORDER_TO_INACTIVE.GetDescription(),
                        Message = string.Format(_systemResourceRepository.GetByResourceCode(MessageCode.W_SHOP_HAVE_ORDER_TO_INACTIVE.GetDescription()),
                            numOfPendingOrder,
                            numOfConfirmOrder,
                            numOfPreparingOrder),
                    });
                }
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                // Process cancel order
                var listOrderProcessing = _orderRepository.Get(o => OrderConstant.LIST_ORDER_STATUS_IN_PROCESSING.Any(x => x == o.Status)).ToList();
                var numberConfirmOrderOverAHour = 0;
                foreach (var order in listOrderProcessing)
                {
                    if (order.Status == OrderStatus.Pending)
                        await RejectOrderAsync(order).ConfigureAwait(false);

                    if (order.Status == OrderStatus.Confirmed)
                    {
                        await CancelOrderConfirmedAsync(order).ConfigureAwait(false);

                        // Check see is shop cancel order late than 1 hour near time frame
                        var currentTime = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));
                        var currentTimeInMinutes = (currentTime.Hour * 60) + currentTime.Minute;
                        var startTimeInMinutes = TimeUtils.ConvertToMinutes(order.StartTime);
                        var deadlineInMinutes = startTimeInMinutes - currentTimeInMinutes;
                        if (order.IntendedReceiveDate.Date == currentTime.Date)
                        {
                            if (deadlineInMinutes < OrderConstant.TIME_SHOP_CANCEL_ORDER_CONFIRMED_IN_MINUTES)
                                numberConfirmOrderOverAHour++;
                        }
                    }
                }

                // Check order is in 1 hours to warning. if > 3 need to send mail warning, 5 -> flag account.
                var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
                if (numberConfirmOrderOverAHour > 0)
                {
                    var systemConfig = _systemConfigRepository.Get().FirstOrDefault();
                    shop.NumOfWarning += numberConfirmOrderOverAHour;
                    if (shop.NumOfWarning >= 3 && shop.NumOfWarning < systemConfig.MaxWarningBeforeInscreaseFlag)
                    {
                        // Send email for shop
                        _emailService.SendEmailToAnnounceWarningForShop(_currentPrincipalService.CurrentPrincipal, shop.NumOfWarning);
                    }
                    else if (shop.NumOfWarning >= systemConfig.MaxWarningBeforeInscreaseFlag)
                    {
                        // Apply flag for shop account and increase flag
                        var account = _currentAccountService.GetCurrentAccount();
                        account.NumOfFlag += 1;

                        // Send email for shop annouce flag increase
                        if (account.NumOfFlag >= systemConfig.MaxFlagsBeforeBan)
                        {
                            _emailService.SendEmailToAnnounceAccountGotBanned(_currentPrincipalService.CurrentPrincipal, account.FullName);
                            account.Status = AccountStatus.Banned;
                            _accountRepository.Update(account);
                        }
                        else
                        {
                            _emailService.SendEmailToAnnounceApplyFlagForShop(_currentPrincipalService.CurrentPrincipal, account.NumOfFlag);
                        }

                        _accountRepository.Update(account);
                        var accountFlag = new AccountFlag(AccountActionTypes.CancelConfirmOrder, _currentPrincipalService.CurrentPrincipalId.Value);
                        await _accountFlagRepository.AddAsync(accountFlag).ConfigureAwait(false);

                        // Reset warning
                        shop.NumOfWarning = 0;
                    }
                }

                // Update shop status
                shop.Status = ShopStatus.InActive;
                _shopRepository.Update(shop);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

                return Result.Success(new
                {
                    Code = MessageCode.I_SHOP_CHANGE_STATUS_TO_INAC_SUCC.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_CHANGE_STATUS_TO_INAC_SUCC.GetDescription()),
                });
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        // Case change to Active
        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
            shop.Status = ShopStatus.Active;
            _shopRepository.Update(shop);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            return Result.Success(new
            {
                Code = MessageCode.I_SHOP_CHANGE_STATUS_TO_ACTIVE_SUCC.GetDescription(),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_SHOP_CHANGE_STATUS_TO_ACTIVE_SUCC.GetDescription()),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task RejectOrderAsync(Order order)
    {
        order.Status = OrderStatus.Rejected;
        _orderRepository.Update(order);

        // Notification for customer order rejected
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        var noti = _notificationFactory.CreateOrderRejectedNotification(order, shop);
        await _notifierService.NotifyAsync(noti).ConfigureAwait(false);
    }

    private async Task CancelOrderConfirmedAsync(Order order)
    {
        order.Status = OrderStatus.Cancelled;
        _orderRepository.Update(order);

        // Notification for customer order cancelled
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        var noti = _notificationFactory.CreateOrderCancelNotification(order, shop);
        await _notifierService.NotifyAsync(noti).ConfigureAwait(false);
    }

    private async Task ValidateAsync(UpdateShopStatusInactiveActiveCommand request)
    {
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId);
        if (shop.Status != ShopStatus.Active && request.Status == (int)ShopStatus.InActive)
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_ABLE_TO_IN_ACTIVE.GetDescription());

        if (shop.Status != ShopStatus.InActive && request.Status == (int)ShopStatus.Active)
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_ABLE_TO_ACTIVE.GetDescription());
    }
}