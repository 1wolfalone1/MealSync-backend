﻿using FluentValidation;

namespace MealSync.Application.UseCases.DeliveryPackages.Queries.GetListTimeFrameUnAssigns;

public class GetListTimeFrameUnAssignValidator : AbstractValidator<GetListTimeFrameUnAssignQuery>
{
    public GetListTimeFrameUnAssignValidator()
    {
        RuleFor(x => x.IntendedReceiveDate)
            .Must(x => x != default)
            .WithMessage("Vui lòng cung cấp ngày giao hàng");
    }
}