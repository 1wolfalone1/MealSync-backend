using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Application.Shared;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Orders.Commands.UpdatePaymentStatusIPN;

public class UpdatePaymentStatusIPNCommand : ICommand<VnPayIPNResponse>
{
    public IQueryCollection Query { get; set; }
}