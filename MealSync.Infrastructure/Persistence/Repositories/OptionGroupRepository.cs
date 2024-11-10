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

    public (int TotalCount, List<OptionGroup> OptionGroups) GetAllShopOptonGroup(long? currentPrincipalId, int requestPageIndex, int requestPageSize)
    {
        var query = DbSet
            .Include(op => op.FoodOptionGroups)
            .Include(op => op.Options)
            .Where(op => op.ShopId == currentPrincipalId.Value && op.Status != OptionGroupStatus.Delete);

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
            ? DbSet.Any(og => og.Title == title && og.ShopId == shopId && og.Status != OptionGroupStatus.Delete)
            : DbSet.Any(og => og.Title == title && og.ShopId == shopId && og.Id != id && og.Status != OptionGroupStatus.Delete);
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

    public List<OptionGroup> GetOptionGroupsWithFoodLinkStatus(long shopId, long foodId, int filterMode)
    {
        var query = DbSet.Include(op => op.FoodOptionGroups.Where(fop => fop.FoodId == foodId))
            .Include(op => op.Options.Where(o => o.Status != OptionStatus.Delete))
            .Where(op => op.ShopId == shopId && op.Status != OptionGroupStatus.Delete).AsEnumerable();

        // Get only unlinked with foodId
        if (filterMode == 1)
        {
            query = query.Where(op => op.FoodOptionGroups.All(fog => fog.FoodId != foodId));
        }

        // Get only linked with foodId
        if (filterMode == 2)
        {
            query = query.Where(op => op.FoodOptionGroups.Any(fog => fog.FoodId == foodId));
        }

        return query.OrderBy(fog => fog.UpdatedDate).ToList();
    }
}