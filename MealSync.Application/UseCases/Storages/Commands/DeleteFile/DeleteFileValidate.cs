using FluentValidation;

namespace MealSync.Application.UseCases.Storages.Commands.DeleteFile;

public class DeleteFileValidate : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileValidate()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .WithMessage("Url bắt buộc");
    }
}