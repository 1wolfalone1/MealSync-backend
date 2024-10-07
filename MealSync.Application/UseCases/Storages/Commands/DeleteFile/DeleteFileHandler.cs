using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.Storages.Commands.DeleteFile;

public class DeleteFileHandler : ICommandHandler<DeleteFileCommand, Result>
{
    private readonly IStorageService _storageService;

    public DeleteFileHandler(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<Result<Result>> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var isSuccess = await _storageService.DeleteFileAsync(request.Url).ConfigureAwait(false);
        if (isSuccess)
        {
            return Result.Success(new
            {
                Message = $"Deleted: {request.Url}",
            });
        }
        else
        {
            throw new("Internal Server Error");
        }
    }
}