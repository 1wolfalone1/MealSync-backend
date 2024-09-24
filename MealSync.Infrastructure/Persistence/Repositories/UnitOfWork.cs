using Microsoft.EntityFrameworkCore;
using MealSync.Application.Common.Repositories;
using MealSync.Infrastructure.Persistence.Contexts;
using MealSync.Infrastructure.Persistence.Interceptors;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private const string ErrorNotOpenTransaction = "You not open transaction yet!";
    private const string ErrorAlreadyOpenTransaction = "Transaction already open";
    private bool isTransaction;
    private readonly MealSyncContext context;

    public UnitOfWork(MealSyncContext context)
    {
        this.context = context;
    }

    public bool IsTransaction => isTransaction;

    internal MealSyncContext Context => context;

    public async Task BeginTransactionAsync()
    {
        if (isTransaction)
        {
            throw new Exception(ErrorAlreadyOpenTransaction);
        }

        isTransaction = true;
        await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (!isTransaction)
        {
            throw new Exception(ErrorNotOpenTransaction);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
        await context.Database.CommitTransactionAsync();
        isTransaction = false;
    }

    public void RollbackTransaction()
    {
        if (!isTransaction)
        {
            throw new Exception(ErrorNotOpenTransaction);
        }

        context.Database.RollbackTransaction();
        isTransaction = false;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }
    }

    public async Task SaveChangesAsync()
    {
        if (!isTransaction)
        {
            throw new InvalidOperationException(ErrorNotOpenTransaction);
        }

        await context.SaveChangesAsync().ConfigureAwait(false);
    }
}