using System.Windows.Input;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Reviews.Commands.ShopReplyReviewOfCustomers;

public class ShopReplyReviewOfCustomerCommand : ICommand<Result>
{
    public long OrderId { get; set; }

    public string? Comment { get; set; }

    public string[] ImageUrls { get; set; } = new string[0];
}