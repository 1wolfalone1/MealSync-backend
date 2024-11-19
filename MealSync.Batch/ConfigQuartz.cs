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
            
            options.AddJob<HaftHourlyBatchMarkDeliveryFailJob>(JobKey.Create(nameof(HaftHourlyBatchMarkDeliveryFailJob)))
                .AddTrigger(trigger => trigger.ForJob(JobKey.Create(nameof(HaftHourlyBatchMarkDeliveryFailJob)))
                    .StartNow()
                    .WithCronSchedule(
                        "0 0/30 * * * ?" // Run at the start of every 30 hours
                    )
                );
            
            options.AddJob<UpdateStatusPendingPaymentJob>(JobKey.Create(nameof(UpdateStatusPendingPaymentJob)))
                .AddTrigger(trigger => trigger
                    .ForJob(JobKey.Create(nameof(UpdateStatusPendingPaymentJob)))
                    .WithCronSchedule("0 0/30 * * * ?")); // Runs every 30 minutes

            options.AddJob<HaftHourlyBatchPlusIncomingShopWallet>(JobKey.Create(nameof(HaftHourlyBatchPlusIncomingShopWallet)))
                .AddTrigger(trigger => trigger
                    .ForJob(JobKey.Create(nameof(HaftHourlyBatchPlusIncomingShopWallet)))
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