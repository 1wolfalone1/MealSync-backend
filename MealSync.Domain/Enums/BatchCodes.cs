using System.ComponentModel;

namespace MealSync.Domain.Enums;

public enum BatchCodes
{
    [Description("TestBatch")]
    TestBatch = 1,
    [Description("BatchCheduleMarkDeliveryFail")]
    BatchCheduleMarkDeliveryFail = 2,
    [Description("BatchCheduleTransferMoneyToShopWallet")]
    BatchCheduleTransferMoneyToShopWallet = 3,
}