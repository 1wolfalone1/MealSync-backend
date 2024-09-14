using Microsoft.EntityFrameworkCore;
using MealSync.Application.Common.Repositories;
using MealSync.Infrastructure.Persistence.Contexts;

namespace MealSync.Infrastructure.Persistence.Repositories;
public class UnitOfWork : IUnitOfWork
{
    private const string ErrorNotOpenTransaction = "You not open transaction yet!";
    private const string ErrorAlreadyOpenTransaction = "Transaction already open";
    private bool isTransaction;
    private MealSyncContext context;

    public UnitOfWork()
    {
        this.context = new MealSyncContext();
    }

    public bool IsTransaction
    {
        get
        {
            return this.isTransaction;
        }
    }

    internal MealSyncContext Context { get => this.context; }

    public async Task BeginTransactionAsync()
    {
        if (this.isTransaction)
        {
            throw new Exception(ErrorAlreadyOpenTransaction);
        }

        isTransaction = true;
    }

    public async Task CommitTransactionAsync()
    {
        if (!this.isTransaction)
        {
            throw new Exception(ErrorNotOpenTransaction);
        }

        await this.context.SaveChangeAsync().ConfigureAwait(false);
        this.isTransaction = false;
    }

    public void RollbackTransaction()
    {
        if (!this.isTransaction)
        {
            throw new Exception(ErrorNotOpenTransaction);
        }

        this.isTransaction = false;

        foreach (var entry in this.context.ChangeTracker.Entries())
        {
            entry.State = EntityState.Detached;
        }
    }
}
