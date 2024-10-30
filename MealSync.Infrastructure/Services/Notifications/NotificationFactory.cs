using AutoMapper;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Services.Notifications.Models;
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
}