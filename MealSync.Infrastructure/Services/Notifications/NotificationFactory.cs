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
}