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
}