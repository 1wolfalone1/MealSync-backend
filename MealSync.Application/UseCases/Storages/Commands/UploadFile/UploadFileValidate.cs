using FluentValidation;

namespace MealSync.Application.UseCases.Storages.Commands.UploadFile;

public class UploadFileValidate : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidate()
    {
        RuleFor(x => x.File)
            .NotEmpty()
            .WithMessage("File không được rỗng.")
            .Must(file => file.Length <= 5 * 1024 * 1024)
            .WithMessage("Không được vượt quá 5 MB.");
    }
}