using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Storages.Commands.UploadFile;

public class UploadFileHandler : ICommandHandler<UploadFileCommand, Result>
{
    private readonly IStorageService _storageService;

    public UploadFileHandler(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<Result<Result>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var url = await _storageService.UploadFileAsync(request.File, request.IsCheckFoodDrink).ConfigureAwait(false);
        return Result.Create(new
        {
            Url = url,
        });
    }
}