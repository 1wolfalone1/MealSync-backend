using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Requests;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.WithdrawalRequests.Queries.GetAllWithdrawalRequestForMod;

public class GetAllWithdrawalRequestForModQuery : PaginationRequest, IQuery<Result>
{
    public string? SearchValue { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public WithdrawalRequestStatus? Status { get; set; }

    public long? DormitoryId { get; set; }

    public FilterWithdrawalRequestOrderBy OrderBy { get; set; } = FilterWithdrawalRequestOrderBy.CreatedDate;

    public FilterWithdrawalRequestDirection Direction { get; set; } = FilterWithdrawalRequestDirection.DESC;

    public enum FilterWithdrawalRequestOrderBy
    {
        CreatedDate = 1,
        ShopName = 2,
        RequestAmount = 3,
        AvailableAmount = 4,
        BankCode = 5,
        BankAccountNumber = 6,
    }

    public enum FilterWithdrawalRequestDirection
    {
        ASC = 1,
        DESC = 2,
    }
}