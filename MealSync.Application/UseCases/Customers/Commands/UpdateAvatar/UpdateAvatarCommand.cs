using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Customers.Commands.UpdateAvatar;

public class UpdateAvatarCommand : ICommand<Result>
{
    public IFormFile File { get; set; }
}