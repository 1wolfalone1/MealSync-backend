using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Orders.Commands.ShowQRConfirm;

public class ShowQRConfirmCommand : ICommand<Result>
{
    public long Id { get; set; }
}