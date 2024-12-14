using AutoMapper;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Notifications.Models;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace MealSync.Infrastructure.Services.Notifications;

public class NotificationFactory : INotificationFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotificationFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Notification CreateOrderPendingNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_PENDING.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderConfirmedNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CONFIRMED.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderAutoConfirmedNotification(Order order, Account account)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_AUTO_CONFIRMED.GetDescription(), order.Id),
            ImageUrl = account.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderRejectedNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_REJECT.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderCancelNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CANCEL.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderConfirmNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CONFIRM.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateRefundFaillNotification(Order order, Account accMode)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = accMode.Id,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_REFUND_FAIL.GetDescription(), order.Id),
            ImageUrl = accMode.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToModerator,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderPreparingNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_PREPARING.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderCustomerDeliveredNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CUSTOMER_DELIVERED.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderShopDeliveredNotification(Order order, Account accShip)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        var nameOfDeliver = accShip.Id == order.ShopId ? "bạn" : accShip.FullName;
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_SHOP_DELIVERED.GetDescription(), new string[] { order.Id.ToString(), nameOfDeliver }),
            ImageUrl = accShip.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderCustomerDeliveringNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CUSTOMER_DELIVERING.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderAssignedToStaffNotification(DeliveryPackage deliveryPackage, Account accShip, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var deliveryPackageMapper = mapper.Map<DeliveryPackageNotification>(deliveryPackage);
        return new Notification
        {
            AccountId = accShip.Id,
            ReferenceId = deliveryPackageMapper.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_ASSIGNED_TO_SHOP_STAFF.GetDescription(), deliveryPackage.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(deliveryPackageMapper),
            Type = NotificationTypes.SendToShopDeliveryStaff,
            EntityType = NotificationEntityTypes.DeliveryPackage,
            IsSave = true,
        };
    }

    public Notification CreateOrderDeliveryFailedToCustomerNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_DELIVERY_FAIL_TO_CUSTOMER.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderDeliveryFailedToShopNotification(Order order, Account accShip, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        var nameOfDeliver = accShip.Id == order.ShopId ? "bạn" : accShip.FullName;
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_DELIVERY_FAIL_TO_SHOP.GetDescription(), new object[] { order.Id, nameOfDeliver }),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderDeliveryFailedAutoByBatchToCustomerNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_DELIVERY_FAIL_AUTO_BY_BATCH.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateLimitAvailableAmountAndInActiveShopNotification(Shop shop, Wallet wallet)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var walletNotification = mapper.Map<WalletNotification>(wallet);
        return new Notification
        {
            AccountId = shop.Id,
            ReferenceId = wallet.Id,
            Title = NotificationConstant.WALLET_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_AVAILABLE_AMOUNT_LESS_THAN_LIMIT.GetDescription(), MoneyUtils.FormatMoneyWithDots(wallet.AvailableAmount),
                MoneyUtils.AVAILABLE_AMOUNT_LIMIT) ?? string.Empty,
            ImageUrl = shop.LogoUrl, // Todo: Image warning
            Data = JsonConvert.SerializeObject(walletNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Wallet,
            IsSave = true,
        };
    }

    public Notification CreateWarningFlagCustomerNotification(Account account)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var accountNotification = mapper.Map<AccountNotification>(account);
        return new Notification
        {
            AccountId = account.Id,
            ReferenceId = account.Id,
            Title = NotificationConstant.ACCOUNT_WARNING_FLAG,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_WARNING_FLAG_CUSTOMER.GetDescription(), account.NumOfFlag) ?? string.Empty,
            ImageUrl = account.AvatarUrl, // Todo: Image warning
            Data = JsonConvert.SerializeObject(accountNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Account,
            IsSave = true,
        };
    }

    public Notification CreateUnderReviewCustomerReportNotification(Account customerAccount, Shop shop, Report report)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var reportNotification = mapper.Map<ReportNotification>(report);
        return new Notification
        {
            AccountId = customerAccount.Id,
            ReferenceId = report.OrderId,
            Title = NotificationConstant.REPORT_ORDER,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_UNDER_REVIEW_REPORT.GetDescription(), report.OrderId) ?? string.Empty,
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(reportNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateUnderReviewReportOfShopNotification(Shop shop, Account customerAccount, Report report)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var reportNotification = mapper.Map<ReportNotification>(report);
        return new Notification
        {
            AccountId = shop.Id,
            ReferenceId = report.OrderId,
            Title = NotificationConstant.REPORT_ORDER,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_UNDER_REVIEW_REPORT.GetDescription(), report.OrderId) ?? string.Empty,
            ImageUrl = customerAccount.AvatarUrl,
            Data = JsonConvert.SerializeObject(reportNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateApproveOrRejectCustomerReportNotification(Account customerAccount, Shop shop, Report report, bool isApprove)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var reportNotification = mapper.Map<ReportNotification>(report);
        var content = isApprove
            ? systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_APPROVE_REPORT.GetDescription(), report.OrderId) ?? string.Empty
            : systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_REJECT_REPORT.GetDescription(), report.OrderId) ?? string.Empty;
        return new Notification
        {
            AccountId = customerAccount.Id,
            ReferenceId = report.OrderId,
            Title = NotificationConstant.REPORT_ORDER,
            Content = content,
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(reportNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateApproveOrRejectReportOfShopNotification(Shop shop, Account customerAccount, Report report, bool isApprove)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var reportNotification = mapper.Map<ReportNotification>(report);
        var content = isApprove
            ? systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_APPROVE_REPORT.GetDescription(), report.OrderId) ?? string.Empty
            : systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_REJECT_REPORT.GetDescription(), report.OrderId) ?? string.Empty;
        return new Notification
        {
            AccountId = shop.Id,
            ReferenceId = report.OrderId,
            Title = NotificationConstant.REPORT_ORDER,
            Content = content,
            ImageUrl = customerAccount.AvatarUrl,
            Data = JsonConvert.SerializeObject(reportNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateCustomerReportOrderNotification(Order order, Account accountCustomer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.REPORT_ORDER,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_CUSTOMER_REPORT.GetDescription(), order.Id) ?? string.Empty,
            ImageUrl = accountCustomer.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateShopReplyReportOrderNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.REPORT_ORDER,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_SHOP_REPLY_REPORT.GetDescription(), order.Id) ?? string.Empty,
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Report,
            IsSave = true,
        };
    }

    public Notification CreateCustomerReviewOrderNotification(Order order, Account accountCustomer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.REVIEW_ORDER,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_CUSTOMER_REVIEW.GetDescription(), order.Id) ?? string.Empty,
            ImageUrl = accountCustomer.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateShopReplyReviewOrderNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = shop.Id,
            Title = NotificationConstant.REVIEW_ORDER,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_SHOP_REPLY_REVIEW.GetDescription(), order.Id) ?? string.Empty,
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Review,
            IsSave = true,
        };
    }

    public Notification CreateOrderDeliveryFailedAutoByBatchToShopNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_DELIVERY_FAIL_AUTO_BY_BATCH.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderCancelAutoByBatchToShopNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_CANCEL_ORDER_AUTO_BY_BATCH.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderCancelAutoByBatchToCustomerNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_CANCEL_ORDER_AUTO_BY_BATCH.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateOrderDeliveryFailedToModeratorNotification(Order order, Account accMod, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = accMod.Id,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_DELIVERY_FAIL_TO_MODERATOR.GetDescription(), new object[] { order.Id, shop.Id }),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToModerator,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateWithdrawalRequestToModeratorNotification(WithdrawalRequest withdrawalRequest, Account accMod, Shop shop, string content)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var withdrawalRequestNotification = mapper.Map<WithdrawalRequestNotification>(withdrawalRequest);

        return new Notification
        {
            AccountId = accMod.Id,
            ReferenceId = withdrawalRequest.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = content,
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(withdrawalRequestNotification),
            Type = NotificationTypes.SendToModerator,
            EntityType = NotificationEntityTypes.WithdrawalRequest,
            IsSave = true,
        };
    }

    public Notification CreateOrderDeliveringNotification(Order order, Shop shop)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CUSTOMER_DELIVERING.GetDescription(), order.Id),
            ImageUrl = shop.LogoUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateCustomerCancelOrderNotification(Order order, Account account)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CUSTOMER_CANCEL.GetDescription(), order.Id),
            ImageUrl = account.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateSystemCancelOrderOfCustomerNotification(Order order, Account account)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_OF_CUSTOMER_SYSTEM_CANCEL.GetDescription(), order.Id),
            ImageUrl = account.AvatarUrl, // Todo: System image
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateSystemCancelOrderOfShopNotification(Order order, Account account)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_OF_SHOP_SYSTEM_CANCEL.GetDescription(), order.Id),
            ImageUrl = account.AvatarUrl, // Todo: System image
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateCustomerCompletedOrderNotification(Order order, Account account)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_ORDER_CUSTOMER_CONFIRM_COMPLETED.GetDescription(), order.Id),
            ImageUrl = account.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateShopWalletReceiveIncommingAmountNotification(Order order, Account account, double amountPlus)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.WALLET_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_WALLET_SHOP_RECEIVE_INCOMMING_AMOUNT.GetDescription(), new object[] { MoneyUtils.FormatMoneyWithDots(amountPlus), order.Id }),
            ImageUrl = account.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateTakeCommissionFromShopWalletNotification(Order order, Account account, double amountTake)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.ShopId,
            ReferenceId = order.Id,
            Title = NotificationConstant.WALLET_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_WALLET_TAKE_COMMISISSION_FEE_FROM_SHOP_WALLET.GetDescription(),
                new object[] { MoneyUtils.FormatMoneyWithDots(amountTake), order.Id }),
            ImageUrl = account.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToShop,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateRefundCustomerNotification(Order order, Account account, double amountRefund)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.ORDER_TITLE,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_REFUND_ORDER_FOR_CUSTOMER.GetDescription(),
                new object[] { order.Id, MoneyUtils.FormatMoneyWithDots(amountRefund) }),
            ImageUrl = account.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Order,
            IsSave = true,
        };
    }

    public Notification CreateJoinRoomToCustomerNotification(Order order, Account accountJoin)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var shopRepository = scope.ServiceProvider.GetRequiredService<IShopRepository>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        var identityRoleName = accountJoin.RoleId == (int)Roles.ShopDelivery ? "Shipper" : (accountJoin.RoleId == (int)Roles.ShopOwner ? "Cửa Hàng" : "Khách Hàng");
        var nameMessageFrom = accountJoin.RoleId == (int)Roles.ShopOwner ? shopRepository.GetById(accountJoin.Id).Name : accountJoin.FullName;

        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.OPEN_CHAT,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_JOIN_ROOM_TO_CUSTOMER.GetDescription(),
                new object[] { string.Concat(identityRoleName, nameMessageFrom), order.Id }),
            ImageUrl = accountJoin.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Chat,
            IsSave = false,
        };
    }

    public Notification CreateCloseRoomToCustomerNotification(Order order, Account accountJoin)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var systemResourceRepository = scope.ServiceProvider.GetRequiredService<ISystemResourceRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var shopRepository = scope.ServiceProvider.GetRequiredService<IShopRepository>();
        var orderNotification = mapper.Map<OrderNotification>(order);
        var identityRoleName = accountJoin.RoleId == (int)Roles.ShopDelivery ? "Shipper" : (accountJoin.RoleId == (int)Roles.ShopOwner ? "Cửa Hàng" : "Khách Hàng");
        var nameMessageFrom = accountJoin.RoleId == (int)Roles.ShopOwner ? shopRepository.GetById(accountJoin.Id).Name : accountJoin.FullName;

        return new Notification
        {
            AccountId = order.CustomerId,
            ReferenceId = order.Id,
            Title = NotificationConstant.OPEN_CHAT,
            Content = systemResourceRepository.GetByResourceCode(ResourceCode.NOTIFICATION_CLOSE_ROOM_TO_CUSTOMER.GetDescription(),
                new object[] { string.Concat(identityRoleName, nameMessageFrom), order.Id }),
            ImageUrl = accountJoin.AvatarUrl,
            Data = JsonConvert.SerializeObject(orderNotification),
            Type = NotificationTypes.SendToCustomer,
            EntityType = NotificationEntityTypes.Chat,
            IsSave = false,
        };
    }
}