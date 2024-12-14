using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum BatchCodes
{
    [Description("TestBatch")]
    TestBatch = 1,

    [Description("BatchCheduleMarkDeliveryFail")]
    BatchCheduleMarkDeliveryFail = 2,

    [Description("BatchUpdateStatusPendingPayment")]
    BatchUpdateStatusPendingPayment = 3,

    [Description("BatchUpdateFlagReceiveOrderPauseAndSoldOut")]
    BatchUpdateFlagReceiveOrderPauseAndSoldOut = 4,

    [Description("BatchUpdateCompletedOrder")]
    BatchUpdateCompletedOrder = 5,

    [Description("BatchCheduleTransferMoneyToShopWallet")]
    BatchCheduleTransferMoneyToShopWallet = 6,

    [Description("BatchCheduleCancelOrderOverTimeFrame")]
    BatchCheduleCancelOrderOverTimeFrame = 7,

    [Description("BatchCheduleCloseRoomChat")]
    BatchCheduleCloseRoomChat = 8,
}