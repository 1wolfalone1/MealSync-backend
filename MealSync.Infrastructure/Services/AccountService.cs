using Microsoft.Extensions.Logging;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;

namespace MealSync.Infrastructure.Services;

public class AccountService : IAccountService
{
    private readonly ILogger<AccountService> logger;

    public AccountService(ILogger<AccountService> logger)
    {
        this.logger = logger;
    }

    public void TestWriteLog()
    {
        this.logger.LogInformation($"password: {BCrypUnitls.Hash("1")}");
        this.logger.LogInformation("Log successly");
    }
}
