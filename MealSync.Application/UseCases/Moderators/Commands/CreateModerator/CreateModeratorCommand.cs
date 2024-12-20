﻿using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Moderators.Commands.CreateModerator;

public class CreateModeratorCommand : ICommand<Result>
{
    public string FullName { get; set; }

    public string Email { get; set; }

    public AccountStatus Status { get; set; }

    public string PhoneNumber { get; set; }

    public long[] DormitoryIds { get; set; }
}