using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetListAccount;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<(List<AccountForModManageDto> Customers, int TotalCount)> GetAllCustomer(
        List<long> dormitoryIds, string? searchValue, DateTime? dateFrom,
        DateTime? dateTo, CustomerStatus? status, long? dormitoryId,
        GetListAccountQuery.FilterCustomerOrderBy? orderBy, GetListAccountQuery.FilterCustomerDirection? direction, int pageIndex, int pageSize);

    Task<AccountDetailForModManageDto?> GetCustomerDetail(List<long> dormitoryIds, long customerId);

    Task<Customer?> GetCustomer(List<long> dormitoryIds, long customerId);

    Task<Customer> GetIncludeAccount(long id);
}