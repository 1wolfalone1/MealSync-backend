using Microsoft.Extensions.Logging;
using MealSync.Application.Common.Services;

namespace MealSync.Infrastructure.Services;

public class TestService : BaseService, ITestService
{
    private readonly ILogger<TestService> logger;

    public TestService(ILogger<TestService> logger)
    {
        this.logger = logger;
    }

    public void TestWriteLog()
    {
        logger.LogInformation("Test Log");
    }
}
