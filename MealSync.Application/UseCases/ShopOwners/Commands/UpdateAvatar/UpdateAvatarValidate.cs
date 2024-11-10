using FluentValidation;

namespace MealSync.Application.UseCases.ShopOwners.Commands.UpdateAvatar;

public class UpdateAvatarValidate : AbstractValidator<UpdateAvatarCommand>
{
    public UpdateAvatarValidate()
    {
        RuleFor(x => x.File)
            .Must(file => file != default && file.Length <= 5 * 1024 * 1024)
            .WithMessage("Không được rỗng và vượt quá 5 MB.");
    }
}