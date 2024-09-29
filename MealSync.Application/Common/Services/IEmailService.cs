namespace MealSync.Application.Common.Services;

public interface IEmailService
{
    bool SendVerificationCodeRegister(string email, string code);

    bool SendVerificationCodeForgotPassword(string email, string code);
}