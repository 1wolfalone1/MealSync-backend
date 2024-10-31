using System.Net.Mail;
using System.Net;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
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

    public bool SendEmailToAnnounceWarningForShop(string email, int numberOfWarning)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_ANNOUCE_WARNING_FOR_SHOP.GetDescription(), numberOfWarning),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_ANNOUCE_WARNING_FOR_SHOP.GetDescription(), numberOfWarning));
    }

    public bool SendEmailToAnnounceApplyFlagForShop(string email, int flag)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_SUBJECT_ANNOUCE_APPLY_FLAG_FOR_SHOP.GetDescription(), flag),
            _systemResourceRepository.GetByResourceCode(ResourceCode.EMAIL_BODY_ANNOUCE_APPLY_FLAG_FOR_SHOP.GetDescription(), flag));
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

    private bool SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            var fromMail = _configuration["EMAIL"] ?? "";
            var fromPassword = _configuration["EMAIL_PASSWORD"] ?? "";

            var message = new MailMessage();
            message.From = new MailAddress(fromMail);
            message.Subject = subject;
            message.To.Add(new MailAddress(toEmail));
            message.Body = body;
            message.IsBodyHtml = true;

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