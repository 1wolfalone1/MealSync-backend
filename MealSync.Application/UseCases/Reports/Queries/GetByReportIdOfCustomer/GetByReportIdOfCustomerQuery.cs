using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Reports.Queries.GetByReportIdOfCustomer;

public class GetByReportIdOfCustomerQuery : IQuery<Result>
{
    public long CustomerReportId { get; set; }
}