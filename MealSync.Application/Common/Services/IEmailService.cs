namespace MealSync.Application.Common.Services;

public interface IEmailService
{
    bool SendVerificationCodeRegister(string email, string code);

    bool SendVerificationCodeForgotPassword(string email, string code);

    bool SendVerificationCodeOldEmail(string email, string code);

    bool SendVerificationCodeUpdateEmail(string email, string code);

    bool SendEmailToAnnounceWarningForShop(string email, int numberOfWarning);

    bool SendEmailToAnnounceApplyFlagForShop(string email, int flag);

    bool SendEmailToAnnounceModeratorRefundFail(string email, long orderId);

    bool SendEmailToAnnounceAccountGotBanned(string email, string fullName);

    bool SendVerificationCodeWithdrawalRequest(string email, string code, string amount);
}