using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Reports.Commands.UpdateReportStatusForMod;

public class UpdateReportStatusForModCommand : ICommand<Result>
{
    public long Id { get; set; }

    public ReportStatus Status { get; set; }
}