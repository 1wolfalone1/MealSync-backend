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
            _systemResourceRepository.GetByResourceCode(MessageCode.I_EMAIL_SUBJECT_REGISTER_VERIFICATION.GetDescription(), code),
            _systemResourceRepository.GetByResourceCode(MessageCode.I_EMAIL_REGISTER_VERIFICATION.GetDescription(), code));
    }

    public bool SendVerificationCodeForgotPassword(string email, string code)
    {
        return SendEmail(email,
            _systemResourceRepository.GetByResourceCode(MessageCode.I_EMAIL_SUBJECT_FORGOT_PASSWORD_VERIFICATION.GetDescription(), code),
            _systemResourceRepository.GetByResourceCode(MessageCode.I_EMAIL_FORGOT_PASSWORD_VERIFICATION.GetDescription(), code));
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