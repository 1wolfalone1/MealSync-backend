using System.ComponentModel;

namespace MealSync.API.Shared;

public enum WhiteEndpointsAcceptBanning
{
    // Customer
    // [Description]
    // CUSTOMER_DELIVERIED = 1,
    [Description("/api/v1/food")]
    CUSTOMER_FOOD_IN_CART,

    [Description("/api/v1/shop-owner/order/delivering")]
    SHOP_DELIVERING_ORDER,

    [Description("/api/v1/shop-owner-staff/order/delivering")]
    SHOP_STAFF_DELIVERING_ORDER,

    [Description("/api/v1/shop-owner/order/{id:long}/delivered")]
    SHOP_DELIVERD_ORDER,

    [Description("/api/v1/shop-owner-staff/order/{id:long}/delivered")]
    SHOP_SHOP_DELIVERD_ORDER,

    [Description("/api/v1/shop-owner-staff/order/{id:long}/delivery-fail")]
    SHOP_FAIL_DELIVERY_ORDER,

    [Description("/api/v1/shop-owner/delivery-package")]
    SHOP_CREATE_DELIVERY_PACKAGE_GROUP,

    [Description("/api/v1/shop-owner/delivery-package-group")]
    SHOP_UPDATE_DELIVERY_PACKAGE_GROUP,

    [Description("/api/v1/shop-owner/order/{id:long}/assign")]
    SHOP_ASSIGN_ORDER,

    [Description("/api/v1/shop-owner/order/{id:long}/un-assign")]
    SHOP_UNASSIGN_ORDER,
}