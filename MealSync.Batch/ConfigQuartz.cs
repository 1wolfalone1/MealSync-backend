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
                            schedule.WithIntervalInMinutes(10).RepeatForever()));

            options.AddJob<HourlyBatchMarkDeliveryFailJob>(JobKey.Create(nameof(HourlyBatchMarkDeliveryFailJob)))
                .AddTrigger(trigger => trigger.ForJob(JobKey.Create(nameof(HourlyBatchMarkDeliveryFailJob)))
                    .StartNow()
                    .WithCronSchedule(
                        "0 0 0/2 * * ?", // Run at the start of every 2 hours
                        x => x.InTimeZone(TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")) // Set timezone to UTC+7
                    )
                );

            options.AddJob<UpdateStatusPendingPaymentJob>(JobKey.Create(nameof(UpdateStatusPendingPaymentJob)))
                .AddTrigger(trigger => trigger
                    .ForJob(JobKey.Create(nameof(UpdateStatusPendingPaymentJob)))
                    .WithCronSchedule("0 0/30 * * * ?")); // Runs every 30 minutes

            options.AddJob<UpdateCompletedOrderJob>(JobKey.Create(nameof(UpdateCompletedOrderJob)))
                .AddTrigger(trigger => trigger
                    .ForJob(JobKey.Create(nameof(UpdateCompletedOrderJob)))
                    .WithCronSchedule("0 0/30 * * * ?")); // Runs every 30 minutes

            options.AddJob<UpdateFlagReceiveOrderPauseAndSoldOutJob>(JobKey.Create(nameof(UpdateFlagReceiveOrderPauseAndSoldOutJob)))
                .AddTrigger(trigger => trigger
                    .ForJob(JobKey.Create(nameof(UpdateFlagReceiveOrderPauseAndSoldOutJob)))
                    .WithCronSchedule("0 0 0 * * ?"));
        });

        services.AddQuartzHostedService();

        return services;
    }
}