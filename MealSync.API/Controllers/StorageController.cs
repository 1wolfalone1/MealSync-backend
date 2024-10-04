using MealSync.API.Shared;
using MealSync.Application.UseCases.Storages.Commands.DeleteFile;
using MealSync.Application.UseCases.Storages.Commands.UploadFile;
using Microsoft.AspNetCore.Mvc;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class StorageController : BaseApiController
{
    [HttpPut(Endpoints.UPLOAD_FILE)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        return HandleResult(await Mediator.Send(new UploadFileCommand { File = file }).ConfigureAwait(false));
    }

    [HttpDelete(Endpoints.DELETE_FILE)]
    public async Task<IActionResult> DeleteFile(string url)
    {
        return HandleResult(await Mediator.Send(new DeleteFileCommand() { Url = url }).ConfigureAwait(false));
    }
}