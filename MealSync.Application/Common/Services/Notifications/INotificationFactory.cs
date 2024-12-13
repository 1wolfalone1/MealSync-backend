using MealSync.Domain.Entities;

namespace MealSync.Application.Common.Services.Notifications;

public interface INotificationFactory
{
    Notification CreateOrderPendingNotification(Order order, Shop shop);

    Notification CreateOrderConfirmedNotification(Order order, Shop shop);

    Notification CreateOrderAutoConfirmedNotification(Order order, Account account);

    Notification CreateOrderRejectedNotification(Order order, Shop shop);

    Notification CreateOrderCancelNotification(Order order, Shop shop);

    Notification CreateOrderConfirmNotification(Order order, Shop shop);

    Notification CreateRefundFaillNotification(Order order, Account accMod);

    Notification CreateOrderPreparingNotification(Order order, Shop shop);

    Notification CreateOrderCustomerDeliveredNotification(Order order, Shop shop);

    Notification CreateOrderShopDeliveredNotification(Order order, Account accShip);

    Notification CreateOrderCustomerDeliveringNotification(Order order, Shop shop);

    Notification CreateOrderAssignedToStaffNotification(DeliveryPackage deliveryPackage, Account accShip, Shop shop);

    Notification CreateOrderDeliveryFailedToCustomerNotification(Order order, Shop shop);

    Notification CreateOrderDeliveryFailedToShopNotification(Order order, Account accShip, Shop shop);

    Notification CreateOrderDeliveryFailedToModeratorNotification(Order order, Account accMod, Shop shop);

    Notification CreateWithdrawalRequestToModeratorNotification(WithdrawalRequest withdrawalRequest, Account accMod, Shop shop, string content);

    Notification CreateOrderDeliveringNotification(Order order, Shop shop);

    Notification CreateCustomerCancelOrderNotification(Order order, Account account);

    Notification CreateCustomerCompletedOrderNotification(Order order, Account account);

    Notification CreateOrderDeliveryFailedAutoByBatchToShopNotification(Order order, Shop shop);

    Notification CreateOrderDeliveryFailedAutoByBatchToCustomerNotification(Order order, Shop shop);

    Notification CreateLimitAvailableAmountAndInActiveShopNotification(Shop shop, Wallet wallet);

    Notification CreateShopWalletReceiveIncommingAmountNotification(Order order, Account account, double amountPlus);

    Notification CreateTakeCommissionFromShopWalletNotification(Order order, Account account, double amountTake);

    Notification CreateWarningFlagCustomerNotification(Account account);

    Notification CreateSystemCancelOrderOfCustomerNotification(Order order, Account account);

    Notification CreateSystemCancelOrderOfShopNotification(Order order, Account account);

    Notification CreateRefundCustomerNotification(Order order, Account account, double amountRefund);

    Notification CreateUnderReviewCustomerReportNotification(Account customerAccount, Shop shop, Report report);

    Notification CreateUnderReviewReportOfShopNotification(Shop shop, Account customerAccount, Report report);

    Notification CreateApproveOrRejectCustomerReportNotification(Account customerAccount, Shop shop, Report report, bool isApprove);

    Notification CreateApproveOrRejectReportOfShopNotification(Shop shop, Account customerAccount, Report report, bool isApprove);

    Notification CreateCustomerReportOrderNotification(Order order, Account accountCustomer);

    Notification CreateShopReplyReportOrderNotification(Order order, Shop shop);

    Notification CreateCustomerReviewOrderNotification(Order order, Account accountCustomer);

    Notification CreateShopReplyReviewOrderNotification(Order order, Shop shop);

    Notification CreateOrderCancelAutoByBatchToCustomerNotification(Order order, Shop shop);

    Notification CreateOrderCancelAutoByBatchToShopNotification(Order order, Shop shop);

    Notification CreateJoinRoomToCustomerNotification(Order order, Account accountJoin);
}