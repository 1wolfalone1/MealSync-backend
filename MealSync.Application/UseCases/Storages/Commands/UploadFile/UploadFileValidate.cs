using FluentValidation;

namespace MealSync.Application.UseCases.Storages.Commands.UploadFile;

public class UploadFileValidate : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidate()
    {
        RuleFor(x => x.File)
            .Must(file => file != default && file.Length <= 10 * 1024 * 1024)
            .WithMessage("Không được rỗng và vượt quá 5 MB.");
    }
}