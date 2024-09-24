﻿namespace MealSync.Application.Common.Repositories;

public interface IUnitOfWork
{
    bool IsTransaction { get; }

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    void RollbackTransaction();

    Task SaveChangesAsync();
}