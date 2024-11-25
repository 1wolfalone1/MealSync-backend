namespace MealSync.Application.UseCases.Wallets.Models;

public class WithdrawalRequestCacheDto
{
    public string Code { get; set; }

    public WithdrawalRequestDto Data { get; set; }

    public class WithdrawalRequestDto
    {
        public long Amount { get; set; }

        public string BankCode { get; set; }

        public string BankShortName { get; set; }

        public string BankAccountNumber { get; set; }

        public string BankAccountName { get; set; }
    }
}