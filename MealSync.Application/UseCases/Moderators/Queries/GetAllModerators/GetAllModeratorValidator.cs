using FluentValidation;

namespace MealSync.Application.UseCases.Moderators.Queries.GetAllModerators;

public class GetAllModeratorValidator : AbstractValidator<GetAllModeratorQuery>
{
    public GetAllModeratorValidator()
    {
    }
}