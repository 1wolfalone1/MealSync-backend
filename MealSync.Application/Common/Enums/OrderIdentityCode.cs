using System.ComponentModel;

namespace MealSync.Application.Common.Enums;

public enum OrderIdentityCode
{
    // Cancel
    [Description("ShopCancel")]
    ORDER_IDENTITY_SHOP_CANCEL,

    [Description("CustomerCancel")]
    ORDER_IDENTITY_CUSTOMER_CANCEL,

    // Delivery Fail
    [Description("DeliveryFailByShop")]
    ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP,

    [Description("DeliveryFailByCustomer")]
    ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER,

    [Description("DeliveryFailReportedByCustomer")]
    ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER,

    [Description("DeliveredReportedByCustomer")]
    ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER,
}