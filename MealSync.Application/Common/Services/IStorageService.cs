using Microsoft.AspNetCore.Http;

namespace MealSync.Application.Common.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file);

    Task<bool> DeleteFileAsync(string url);
}