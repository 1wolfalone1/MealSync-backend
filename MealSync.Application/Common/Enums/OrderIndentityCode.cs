using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum OrderIndentityCode
{
    // Cancel
    [Description("Order-Identity-ShopCancel")]
    ORDER_IDENTITY_SHOP_CANCEL,

    [Description("Order-Identity-CustomerCancel")]
    ORDER_IDENTITY_CUSTOMER_CANCEL,

    // Delivery Fail
    [Description("Order-Identity-DeliveryFailByShop")]
    ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP,

    [Description("Order-Identity-DeliveryFailByCustomer")]
    ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER,
}