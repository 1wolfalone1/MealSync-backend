using MealSync.Application.Common.Enums;
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

    public async Task<(List<AccountForModManageDto> Customers, int TotalCount)> GetAllCustomer(
        List<long> dormitoryIds, string? searchValue,
        DateTime? dateFrom, DateTime? dateTo, CustomerStatus? status, long? dormitoryId,
        GetListAccountQuery.FilterCustomerOrderBy? orderBy, GetListAccountQuery.FilterCustomerDirection? direction, int pageIndex, int pageSize)
    {
        var query = DbSet
            .Where(c => c.Account.Status != AccountStatus.Deleted && c.Account.Status != AccountStatus.UnVerify
                                                                  && (c.CustomerBuildings.Any(cb => dormitoryIds.Contains(cb.Building.DormitoryId))
                                                                      || c.Orders.Any(o => dormitoryIds.Contains(o.Building.DormitoryId)))
            )
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

        if (status.HasValue)
        {
            query = query.Where(customer => customer.Status == status.Value);
        }

        if (dormitoryId.HasValue && dormitoryId > 0)
        {
            query = query.Where(c => c.CustomerBuildings.Any(cb => cb.Building.DormitoryId == dormitoryId));
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
                Status = c.Status,
                CreatedDate = c.CreatedDate,
                TotalOrderInProcess = c.Orders.Count(o => o.Status == OrderStatus.Preparing || o.Status == OrderStatus.Delivering),
            })
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (customers, totalCount);
    }

    public Task<AccountDetailForModManageDto?> GetCustomerDetail(List<long> dormitoryIds, long customerId)
    {
        return DbSet
            .Where(c => c.Account.Status != AccountStatus.Deleted
                        && c.Account.Status != AccountStatus.UnVerify
                        && c.Id == customerId
                        && (
                            c.CustomerBuildings.Any(sd => dormitoryIds.Contains(sd.Building.DormitoryId))
                            || c.Orders.Any(o => dormitoryIds.Contains(o.Building.DormitoryId))
                        )
            ).Select(c => new AccountDetailForModManageDto
            {
                Id = c.Id,
                Email = c.Account.Email,
                PhoneNumber = c.Account.PhoneNumber,
                FullName = c.Account.FullName,
                AvatarUrl = c.Account.AvatarUrl,
                Genders = c.Account.Genders,
                Status = c.Account.Status,
                NumOfFlag = c.Account.NumOfFlag,
                CreatedDate = c.CreatedDate,
                AccountFlags = c.Account.AccountFlags.Select(af => new AccountDetailForModManageDto.AccountFlagDetailDto
                {
                    Id = af.Id,
                    ActionType = af.ActionType,
                    TargetType = af.TargetType,
                    TargetId = af.TargetId,
                    Description = af.Description,
                    CreatedDate = af.CreatedDate,
                }).ToList(),
                OrderSummary = new AccountDetailForModManageDto.OrderSummaryDto
                {
                    TotalOrderInProcess = c.Orders.Sum(o => o.Status != OrderStatus.Rejected
                                                            && o.Status != OrderStatus.Cancelled
                                                            && o.Status != OrderStatus.Completed
                                                            && o.Status != OrderStatus.Resolved
                        ? 1
                        : 0),
                    TotalCancelByCustomer = c.Orders.Sum(o => o.Status == OrderStatus.Cancelled
                                                              && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription()
                        ? 1
                        : 0),
                    TotalCancelOrRejectByShop = c.Orders.Sum(o => o.Status == OrderStatus.Cancelled
                                                                  && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_SHOP_CANCEL.GetDescription()
                        ? 1
                        : o.Status == OrderStatus.Rejected
                            ? 1
                            : 0),
                    TotalDelivered = c.Orders.Sum(o => o.Status == OrderStatus.Completed
                                                       && o.ReasonIdentity == default
                        ? 1
                        : 0),
                    TotalFailDeliveredByCustomer = c.Orders.Sum(o => o.Status == OrderStatus.Completed
                                                                     && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_CUSTOMER.GetDescription()
                        ? 1
                        : 0),
                    TotalFailDeliveredByShop = c.Orders.Sum(o => o.Status == OrderStatus.Completed
                                                                 && o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_BY_SHOP.GetDescription()
                        ? 1
                        : 0),
                    TotalReportResolved = c.Orders.Sum(o => o.Status == OrderStatus.Resolved
                                                            && o.IsReport
                                                            && (o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription()
                                                                || o.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER.GetDescription())
                        ? 1
                        : 0),
                },
            }).FirstOrDefaultAsync();
    }

    public Task<Customer?> GetCustomer(List<long> dormitoryIds, long customerId)
    {
        return DbSet.Include(c => c.Account)
            .Where(c => c.Account.Status != AccountStatus.Deleted
                        && c.Account.Status != AccountStatus.UnVerify
                        && c.Id == customerId
                        && (
                            c.CustomerBuildings.Any(sd => dormitoryIds.Contains(sd.Building.DormitoryId))
                            || c.Orders.Any(o => dormitoryIds.Contains(o.Building.DormitoryId))
                        )
            ).FirstOrDefaultAsync();
    }

    public Task<Customer> GetIncludeAccount(long id)
    {
        return DbSet.Include(c => c.Account).FirstAsync(c => c.Id == id);
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}