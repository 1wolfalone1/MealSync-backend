using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OptionGroupRepository : BaseRepository<OptionGroup>, IOptionGroupRepository
{
    public OptionGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public bool CheckExistedByIdAndShopId(long id, long shopId)
    {
        return DbSet.Any(og => og.Id == id && og.ShopId == shopId && og.Status != OptionGroupStatus.Delete);
    }

    public OptionGroup GetByIdIncludeOption(long id)
    {
        return DbSet.Include(og => og.Options.Where(o => o.Status != OptionStatus.Delete))
            .First(og => og.Id == id && og.Status != OptionGroupStatus.Delete);
    }

    public (int TotalCount, List<OptionGroup> OptionGroups) GetAllShopOptonGroup(long? currentPrincipalId, string? title, int status, int requestPageIndex, int requestPageSize)
    {
        var query = DbSet
            .Include(op => op.FoodOptionGroups)
            .Include(op => op.Options)
            .Where(op => op.ShopId == currentPrincipalId.Value && op.Status != OptionGroupStatus.Delete);

        if (title != default)
        {
            query = query.Where(op => op.Title.Contains(title));
        }

        if (status != 0)
        {
            query = query.Where(op => status == 1 && op.Status == OptionGroupStatus.Active || status == 2 && op.Status == OptionGroupStatus.UnActive);
        }

        var totalCount = query.Count();
        var optionGroups = query
            .OrderByDescending(op => op.CreatedDate)
            .Skip((requestPageIndex - 1) * requestPageSize)
            .Take(requestPageSize)
            .ToList();

        return (totalCount, optionGroups);
    }

    public bool CheckExistTitleOptionGroup(string title, long shopId, long? id = null)
    {
        return id == null
            ? DbSet.Any(og => og.Title == title && og.ShopId == shopId)
            : DbSet.Any(og => og.Title == title && og.ShopId == shopId && og.Id != id);
    }

    public Task<OptionGroup?> GetByIdAndOptionIds(long id, long[] optionIds)
    {
        return DbSet
            .Where(og => og.Id == id && og.Status == OptionGroupStatus.Active)
            .Select(og => new OptionGroup
            {
                Id = og.Id,
                Title = og.Title,
                Options = og.Options
                    .Where(o => optionIds.Contains(o.Id) && o.Status == OptionStatus.Active)
                    .Select(o => new Option
                    {
                        Id = o.Id,
                        Title = o.Title,
                        ImageUrl = o.ImageUrl,
                        IsCalculatePrice = o.IsCalculatePrice,
                        Price = o.Price,
                        Status = o.Status,
                    }).ToList(),
            })
            .FirstOrDefaultAsync();
    }
}