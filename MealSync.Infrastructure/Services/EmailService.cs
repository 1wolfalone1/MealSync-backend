using System.Net.Mail;
using System.Net;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MealSync.Infrastructure.Services;

public class EmailService : IEmailService, IBaseService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, ISystemResourceRepository systemResourceRepository)
    {
        _configuration = configuration;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
    }

    public bool SendVerificationCodeRegister(string email, string code)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_REGISTER_VERIFICATION.GetDescription(), code),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_REGISTER_VERIFICATION.GetDescription(), code));
    }

    public bool SendVerificationCodeForgotPassword(string email, string code)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_FORGOT_PASSWORD_VERIFICATION.GetDescription(), code),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_FORGOT_PASSWORD_VERIFICATION.GetDescription(), code));
    }

    public bool SendVerificationCodeOldEmail(string email, string code)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_OLD_EMAIL_VERIFICATION.GetDescription(), code),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_OLD_EMAIL_VERIFICATION.GetDescription(), code));
    }

    public bool SendVerificationCodeUpdateEmail(string email, string code)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_UPDATE_EMAIL_VERIFICATION.GetDescription(), code),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_UPDATE_EMAIL_VERIFICATION.GetDescription(), code));
    }

    public bool SendEmailToAnnounceWarningForShop(string email, int numberOfWarning)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_ANNOUCE_WARNING_FOR_SHOP.GetDescription(), numberOfWarning),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_ANNOUCE_WARNING_FOR_SHOP.GetDescription(), numberOfWarning));
    }

    public bool SendEmailToAnnounceApplyFlagForShop(string email, int flag, string reason)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_ANNOUCE_APPLY_FLAG_FOR_SHOP.GetDescription(), flag),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_ANNOUCE_APPLY_FLAG_FOR_SHOP.GetDescription(), new object[] { reason, flag }));
    }

    public bool SendEmailToAnnounceModeratorRefundFail(string email, long orderId)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_ANNOUCE_REFUND_ORDER_FAIL.GetDescription(), orderId),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_ANNOUCE_REFUND_ORDER_FAIL.GetDescription(), orderId));
    }

    public bool SendEmailToAnnounceAccountGotBanned(string email, string fullName)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_ACCOUNT_ENOUGH_FLAG_FOR_BAN.GetDescription(), fullName),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_ACCOUNT_ENOUGH_FLAG_FOR_BAN.GetDescription(), fullName));
    }

    public bool SendVerificationCodeWithdrawalRequest(string email, string code, string amount)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_WITHDRAWAL_REQUEST.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_WITHDRAWAL_REQUEST.GetDescription(), amount, code));
    }

    public bool SendNotifyLimitAvailableAmountAndInActiveShop(string email, string availableAmount, string limitAmount)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_LIMIT_AVAILABLE_AMOUNT.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_LIMIT_AVAILABLE_AMOUNT.GetDescription(), availableAmount, limitAmount));
    }

    public bool SendNotifyBannedCustomerAccount(string email, string? fullName, int numberOfFlag)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_BAN_CUSTOMER_ACCOUNT.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_BAN_CUSTOMER_ACCOUNT.GetDescription(), fullName ?? string.Empty, numberOfFlag));
    }

    public bool SendApproveShop(string email, string? fullName, string shopName)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_APPROVE_SHOP.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_APPROVE_SHOP.GetDescription(), fullName, shopName));
    }

    public bool SendBanShopWithReason(string email, string? fullName, string shopName, string reason, int numberOfFlag, bool isBanned)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(isBanned ? ResourceCode.EMAIL_SUBJECT_BANNED_SHOP_WITH_REASON.GetDescription() : ResourceCode.EMAIL_SUBJECT_BANNING_SHOP_WITH_REASON.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_BAN_SHOP_WITH_REASON.GetDescription(), fullName, shopName, reason, numberOfFlag));
    }

    public bool SendUnBanShopWithReason(string email, string? fullName, string shopName, string reason)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_UN_BAN_SHOP_WITH_REASON.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_UN_BAN_SHOP_WITH_REASON.GetDescription(), fullName, shopName, reason));
    }

    public bool SendBanCustomerWithReason(string email, string? fullName, string reason, int numberOfFlag, bool isBanned)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(isBanned ? ResourceCode.EMAIL_SUBJECT_BANNED_CUSTOMER_WITH_REASON.GetDescription() : ResourceCode.EMAIL_SUBJECT_BANNING_CUSTOMER_WITH_REASON.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_BAN_CUSTOMER_WITH_REASON.GetDescription(), fullName, reason, numberOfFlag));
    }

    public bool SendUnBanCustomerWithReason(string email, string? fullName, string reason)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_UN_BAN_CUSTOMER_WITH_REASON.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_UN_BAN_CUSTOMER_WITH_REASON.GetDescription(), fullName, reason));
    }

    public bool SendCreatedAccountModerator(string email, string? fullName, string userName, string password)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_ACCOUNT_FOR_MODERATOR.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_ACCOUNT_FOR_MODERATOR.GetDescription(), fullName, userName, password));
    }

    public bool SendChangeEmailAccountModerator(string oldEmail, string newEmail, string? fullName)
    {
        return SendEmail(oldEmail,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_MODERATOR_ACCOUNT_EMAIL_CHANGE.GetDescription()),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_MODERATOR_ACCOUNT_EMAIL_CHANGE.GetDescription(), fullName, oldEmail, newEmail));
    }

    private bool SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            var fromMail = _configuration["EMAIL"] ?? "";
            var fromPassword = _configuration["EMAIL_PASSWORD"] ?? "";

            var message = new MailMessage
            {
                From = new MailAddress(fromMail, "MealSync"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            message.To.Add(new MailAddress(toEmail));

            var smtpClient = new SmtpClient(_configuration["SMTP_CLIENT"] ?? "")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            smtpClient.Send(message);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return false;
        }
    }
}