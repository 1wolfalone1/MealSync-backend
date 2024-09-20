using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Infrastructure.Persistence.Contexts;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    private const string ErrorMessage = "Haven't any transaction";
    private readonly UnitOfWork unitOfWork;

    public BaseRepository(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork as UnitOfWork;
    }

    public DbSet<TEntity> DbSet => this.unitOfWork.Context.Set<TEntity>();

    public async Task AddAsync(TEntity entity)
    {
        if (!this.unitOfWork.IsTransaction)
        {
            throw new InvalidOperationException(ErrorMessage);
        }

        await this.DbSet.AddAsync(entity).ConfigureAwait(false);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        if (!this.unitOfWork.IsTransaction)
        {
            throw new InvalidOperationException(ErrorMessage);
        }

        await this.DbSet.AddRangeAsync(entities).ConfigureAwait(false);
    }

    public bool Any()
    {
        return this.DbSet.Any();
    }

    public Task<IQueryable<TEntity>> GetAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = false)
    {
        return Task.FromResult(Get(predicate, orderBy, includes, disableTracking));
    }

    public IQueryable<TEntity> Get(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        List<Expression<Func<TEntity, object>>>? includes = null,
        bool disableTracking = false)
    {
        IQueryable<TEntity> query = this.DbSet;

        if (disableTracking)
        {
            query = query.AsNoTracking();
        }

        if (includes != null)
        {
            query = includes.Aggregate(query, (current, include) => current.Include(include));
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return orderBy != null
            ? orderBy(query).AsQueryable()
            : query.AsQueryable();
    }

    public void Remove(TEntity entity)
    {
        if (!this.unitOfWork.IsTransaction)
        {
            throw new InvalidOperationException(ErrorMessage);
        }

        this.DbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        if (!this.unitOfWork.IsTransaction)
        {
            throw new InvalidOperationException(ErrorMessage);
        }

        this.DbSet.RemoveRange(entities);
    }

    public TEntity Update(TEntity entity)
    {
        if (!this.unitOfWork.IsTransaction)
        {
            throw new InvalidOperationException(ErrorMessage);
        }

        this.DbSet.Attach(entity);
        return entity;
    }

    public IEnumerable<TEntity> UpdateRange(IEnumerable<TEntity> entities)
    {
        if (!this.unitOfWork.IsTransaction)
        {
            throw new InvalidOperationException(ErrorMessage);
        }

        this.DbSet.AttachRange(entities);
        return entities;
    }

    public async Task<TEntity?> GetByIdAsync(object id)
    {
        return await this.DbSet.FindAsync(id);
    }

    public TEntity? GetById(object id)
    {
        return this.DbSet.Find(id);
    }
}
