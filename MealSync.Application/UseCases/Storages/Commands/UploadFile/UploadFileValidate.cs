using FluentValidation;

namespace MealSync.Application.UseCases.Storages.Commands.UploadFile;

public class UploadFileValidate : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidate()
    {
        RuleFor(x => x.File)
            .NotEmpty()
            .WithMessage("File không được rỗng.");
    }
}