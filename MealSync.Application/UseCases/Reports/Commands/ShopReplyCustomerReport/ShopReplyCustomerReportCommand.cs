using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Reports.Commands.ShopReplyCustomerReport;

public class ShopReplyCustomerReportCommand : ICommand<Result>
{
    public long ReplyReportId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public List<string> Images { get; set; }
}