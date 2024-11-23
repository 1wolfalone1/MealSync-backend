using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetListAccount;

public class GetListAccountQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public CustomerStatus? Status { get; set; }

    public long? DormitoryId { get; set; }

    public FilterCustomerOrderBy OrderBy { get; set; } = FilterCustomerOrderBy.CreatedDate;

    public FilterCustomerDirection Direction { get; set; } = FilterCustomerDirection.DESC;

    public enum FilterCustomerOrderBy
    {
        CreatedDate = 1,
        FullName = 2,
        Email = 3,
        PhoneNumber = 4,
    }

    public enum FilterCustomerDirection
    {
        ASC = 1,
        DESC = 2,
    }
}