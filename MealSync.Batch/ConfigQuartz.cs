using MealSync.Batch.Schedulers;
using Quartz;

namespace MealSync.Batch;

public static class ConfigQuartz
{
    public static IServiceCollection ConfigureQuartzServices(this IServiceCollection services,
        IConfiguration config)
    {
        // QuartZ
        services.AddQuartz(options =>
        {
            options.UseMicrosoftDependencyInjectionJobFactory();

            var jobKey = JobKey.Create(nameof(DailyBatchCheckerJob));
            options.AddJob<DailyBatchCheckerJob>(jobKey)
                .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                        .WithSimpleSchedule(schedule =>
                            schedule.WithIntervalInMinutes(1).RepeatForever()));

            options.AddJob<HourlyBatchMarkDeliveryFailJob>(JobKey.Create(nameof(HourlyBatchMarkDeliveryFailJob)))
                .AddTrigger(trigger => trigger.ForJob(JobKey.Create(nameof(HourlyBatchMarkDeliveryFailJob)))
                    .WithCronSchedule("0 0,30 * * * ?"));
        });

        services.AddQuartzHostedService();

        return services;
    }
}