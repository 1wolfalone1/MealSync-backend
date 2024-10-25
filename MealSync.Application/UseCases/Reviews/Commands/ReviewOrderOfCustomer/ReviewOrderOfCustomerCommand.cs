using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Reviews.Commands.ReviewOrderOfCustomer;

public class ReviewOrderOfCustomerCommand : ICommand<Result>
{
    public long OrderId { get; set; }

    public RatingRanges Rating { get; set; }

    public string Comment { get; set; }

    public IFormFile[] Images { get; set; }
}