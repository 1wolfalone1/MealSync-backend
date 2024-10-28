using MealSync.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MealSync.Infrastructure.Persistence.Interceptors;

public class CustomSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public CustomSaveChangesInterceptor(ICurrentPrincipalService currentPrincipalService)
    {
        _currentPrincipalService = currentPrincipalService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        var context = eventData.Context;
        foreach (var entry in context.ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
        {
            var now = DateTimeOffset.UtcNow;
            var currentAccountId = this._currentPrincipalService.CurrentPrincipalId;
            entry.Property("UpdatedDate").CurrentValue = now;
            entry.Property("UpdatedBy").CurrentValue = currentAccountId;
            if (entry.State == EntityState.Modified)
            {
                entry.Property("CreatedDate").IsModified = false;
                entry.Property("CreatedBy").IsModified = false;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedDate").CurrentValue = now;
                entry.Property("CreatedBy").CurrentValue = currentAccountId;
            }

            foreach (var property in entry.OriginalValues.Properties)
            {
                if (property.PropertyInfo.PropertyType == typeof(DateTimeOffset))
                {
                    var currentValue = (DateTimeOffset)entry.CurrentValues[property];

                    // Adjust to UTC+7
                    if (currentValue != null)
                        entry.CurrentValues[property] = currentValue;
                }
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
    }
}