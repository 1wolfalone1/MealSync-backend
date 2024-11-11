using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Reports.Commands.CustomerReport;

public class CustomerReportCommand : ICommand<Result>
{
    public long OrderId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public IFormFile[] Images { get; set; }
}