using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MealSync.Application.Common.Services;

public interface IStorageService
{
    Task<string> UploadFileAsync(IFormFile file);

    Task<bool> DeleteFileAsync(string url);

    Task<string> UploadFileAsync(Image<Rgba32> image, string contentType = "image/png");

    Task<Image<Rgba32>> GenerateQRCodeWithLogoAsync(string qrText);
}