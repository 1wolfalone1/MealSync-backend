namespace MealSync.Application.Common.Services;

public interface IEmailService
{
    bool SendVerificationCodeRegister(string email, string code);

    bool SendVerificationCodeForgotPassword(string email, string code);

    bool SendVerificationCodeOldEmail(string email, string code);

    bool SendVerificationCodeUpdateEmail(string email, string code);

    bool SendEmailToAnnounceWarningForShop(string email, int numberOfWarning);

    bool SendEmailToAnnounceApplyFlagForShop(string email, int flag, string reason);

    bool SendEmailToAnnounceModeratorRefundFail(string email, long orderId);

    bool SendEmailToAnnounceAccountGotBanned(string email, string fullName);

    bool SendVerificationCodeWithdrawalRequest(string email, string code, string amount);

    bool SendNotifyLimitAvailableAmountAndInActiveShop(string email, string availableAmount, string limitAmount);

    bool SendNotifyBannedCustomerAccount(string email, string? fullName, int numberOfFlag);

    bool SendApproveShop(string email, string? fullName, string shopName);

    bool SendBanShopWithReason(string email, string? fullName, string shopName, string reason, int numberOfFlag, bool isBanned);

    bool SendUnBanShopWithReason(string email, string? fullName, string shopName, string reason);

    bool SendBanCustomerWithReason(string email, string? fullName, string reason, int numberOfFlag, bool isBanned);

    bool SendUnBanCustomerWithReason(string email, string? fullName, string reason);

    bool SendCreatedAccountModerator(string email, string? fullName, string userName, string password);
}