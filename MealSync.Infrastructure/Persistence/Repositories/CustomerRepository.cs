using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetListAccount;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public async Task<(List<AccountForModManageDto> Customers, int TotalCount)> GetAllCustomerByDormitoryIds(
        List<long> dormitoryIds, string? searchValue,
        DateTime? dateFrom, DateTime? dateTo, AccountStatus? status,
        GetListAccountQuery.FilterCustomerOrderBy? orderBy, GetListAccountQuery.FilterCustomerDirection? direction, int pageIndex, int pageSize)
    {
        var query = DbSet
            .Include(c => c.Account)
            .Where(c => c.Account.Status != AccountStatus.Deleted && c.CustomerBuildings.Any(sd => dormitoryIds.Contains(sd.Building.DormitoryId)))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = int.TryParse(searchValue, out var numericValue);

            query = query.Where(customer =>
                EF.Functions.Like(customer.Account.FullName, $"%{searchValue}%") ||
                EF.Functions.Like(customer.Account.Email, $"%{searchValue}%") ||
                EF.Functions.Like(customer.Account.PhoneNumber, $"%{searchValue}%") ||
                (isNumeric && EF.Functions.Like(customer.Id.ToString(), $"%{numericValue.ToString()}%"))
            );
        }

        if (status.HasValue && status.Value != AccountStatus.Deleted)
        {
            query = query.Where(customer => customer.Account.Status == status.Value);
        }

        if (dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(customer => customer.CreatedDate >= dateFrom.Value && customer.CreatedDate <= dateTo.Value);
        }
        else if (dateFrom.HasValue && !dateTo.HasValue)
        {
            query = query.Where(customer => customer.CreatedDate >= dateFrom.Value);
        }
        else if (!dateFrom.HasValue && dateTo.HasValue)
        {
            query = query.Where(customer => customer.CreatedDate <= dateTo.Value);
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        if (orderBy.HasValue && direction.HasValue)
        {
            if (orderBy.Value == GetListAccountQuery.FilterCustomerOrderBy.CreatedDate)
            {
                query = direction == GetListAccountQuery.FilterCustomerDirection.ASC
                    ? query.OrderBy(shop => shop.CreatedDate)
                    : query.OrderByDescending(shop => shop.CreatedDate);
            }
            else if (orderBy.Value == GetListAccountQuery.FilterCustomerOrderBy.FullName)
            {
                query = direction == GetListAccountQuery.FilterCustomerDirection.ASC
                    ? query.OrderBy(shop => shop.Account.FullName)
                    : query.OrderByDescending(shop => shop.Account.FullName);
            }
            else if (orderBy.Value == GetListAccountQuery.FilterCustomerOrderBy.Email)
            {
                query = direction == GetListAccountQuery.FilterCustomerDirection.ASC
                    ? query.OrderBy(shop => shop.Account.Email)
                    : query.OrderByDescending(shop => shop.Account.Email);
            }
            else if (orderBy.Value == GetListAccountQuery.FilterCustomerOrderBy.PhoneNumber)
            {
                query = direction == GetListAccountQuery.FilterCustomerDirection.ASC
                    ? query.OrderBy(shop => shop.Account.PhoneNumber)
                    : query.OrderByDescending(shop => shop.Account.PhoneNumber);
            }
        }
        else
        {
            query = query.OrderByDescending(shop => shop.CreatedDate);
        }

        var customers = query
            .Select(c => new AccountForModManageDto
            {
                Id = c.Id,
                Email = c.Account.Email,
                PhoneNumber = c.Account.PhoneNumber,
                FullName = c.Account.FullName,
                AvatarUrl = c.Account.AvatarUrl,
                Status = c.Account.Status,
                CreatedDate = c.CreatedDate,
            })
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (customers, totalCount);
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}