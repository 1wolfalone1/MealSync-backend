using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetListAccount;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<(List<AccountForModManageDto> Customers, int TotalCount)> GetAllCustomerByDormitoryIds(
        List<long> dormitoryIds, string? searchValue, DateTime? dateFrom,
        DateTime? dateTo, AccountStatus? status,
        GetListAccountQuery.FilterCustomerOrderBy? orderBy, GetListAccountQuery.FilterCustomerDirection? direction, int pageIndex, int pageSize);
}