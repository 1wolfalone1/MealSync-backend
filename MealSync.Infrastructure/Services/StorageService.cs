using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using MealSync.Application.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace MealSync.Infrastructure.Services;

public class StorageService : IStorageService, IBaseService
{

    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IAmazonS3 _client;

    public StorageService(ILogger<StorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _client = new AmazonS3Client(
            new BasicAWSCredentials(
                _configuration["AWS_ACCESS_KEY"] ?? string.Empty,
                _configuration["AWS_SECRET_KEY"] ?? string.Empty
            ),
            new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(_configuration["AWS_REGION"] ?? string.Empty),
            }
        );
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var bucketName = _configuration["AWS_BUCKET_NAME"] ?? string.Empty;
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms).ConfigureAwait(false);

        // Get file extension
        var fileExtension = Path.GetExtension(file.FileName);

        // Generate file name with extension
        var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid()}{fileExtension}";

        // Set folder based on file type
        if (file.ContentType.StartsWith("image/"))
        {
            fileName = "image/" + fileName;
        }
        else if (file.ContentType.StartsWith("video/"))
        {
            fileName = "video/" + fileName;
        }

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = ms,
            Key = fileName,
            BucketName = bucketName,
            ContentType = file.ContentType,
        };

        var transferUtility = new TransferUtility(_client);
        await transferUtility.UploadAsync(uploadRequest).ConfigureAwait(false);

        var imageUrl = _configuration["AWS_BASE_URL"] + fileName;
        _logger.LogInformation("Push to S3 success with link url {0}", imageUrl);

        return imageUrl;
    }

    public async Task<bool> DeleteFileAsync(string url)
    {
        var bucketName = _configuration["AWS_BUCKET_NAME"] ?? string.Empty;

        // Extract the S3 object key from the URL
        var baseUrl = _configuration["AWS_BASE_URL"];

        // Get the object key (the part after the base URL)
        var key = url.Substring(baseUrl.Length);

        var deleteRequest = new DeleteObjectRequest
        {
            BucketName = bucketName,
            Key = key,
        };

        var response = await _client.DeleteObjectAsync(deleteRequest).ConfigureAwait(false);

        if (response.HttpStatusCode == HttpStatusCode.NoContent)
        {
            _logger.LogInformation("File deleted successfully from S3: {0}", key);
            return true;
        }
        else
        {
            _logger.LogError("Failed to delete file from S3: {0}", key);
            return false;
        }
    }

    public async Task<string> UploadFileAsync(Image<Rgba32> image, string contentType = "image/png")
    {
        var bucketName = _configuration["AWS_BUCKET_NAME"] ?? string.Empty;

        using var ms = new MemoryStream();
        // Save the ImageSharp image to the memory stream in PNG format
        image.Save(ms, new PngEncoder());
        ms.Position = 0; // Reset stream position after writing

        // Use a unique file name based on timestamp and GUID
        var fileName = $"image/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Guid.NewGuid()}.png";
        return await UploadToS3(ms, contentType, fileName);
    }

    // Common upload logic
    private async Task<string> UploadToS3(Stream stream, string contentType, string fileName)
    {
        var bucketName = _configuration["AWS_BUCKET_NAME"] ?? string.Empty;

        var uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            Key = fileName,
            BucketName = bucketName,
            ContentType = contentType
        };

        var transferUtility = new TransferUtility(_client);
        await transferUtility.UploadAsync(uploadRequest).ConfigureAwait(false);

        var imageUrl = _configuration["AWS_BASE_URL"] + fileName;
        _logger.LogInformation("Uploaded to S3 with URL: {0}", imageUrl);

        return imageUrl;
    }

    public async Task<Image<Rgba32>> GenerateQRCodeWithLogoAsync(string qrText)
    {
    // Download the logo from S3
    var bucketName = _configuration["AWS_BUCKET_NAME"] ?? string.Empty;
    var logoKey = _configuration["MEAL_SYNC_LOGO_KEY"] ?? string.Empty;
    Image<Rgba32> logoImage = await GetLogoFromS3Async(bucketName, logoKey);

    // Define the QR code size and initialize the canvas
    int qrCodeSize = 300;
    var qrCodeImage = new Image<Rgba32>(qrCodeSize, qrCodeSize, Color.White);

    // Use ImageSharp.Drawing to create a basic pattern to simulate a QR code
    var blackSquareBrush = Brushes.Solid(Color.Black);
    int cellSize = 20;
    for (int y = 0; y < qrCodeSize; y += cellSize)
    {
        for (int x = 0; x < qrCodeSize; x += cellSize)
        {
            // Simple checkerboard-like pattern, alternate black/white
            if ((x / cellSize + y / cellSize) % 2 == 0)
            {
                qrCodeImage.Mutate(ctx => ctx.Fill(blackSquareBrush, new RectangleF(x, y, cellSize - 5, cellSize - 5)));
            }
        }
    }

    // Resize the logo to fit in the center of the QR code
    int logoSize = qrCodeImage.Width / 4;
    logoImage.Mutate(x => x.Resize(logoSize, logoSize));

    // Center the logo on the QR code
    qrCodeImage.Mutate(ctx =>
    {
        var centerPosition = new Point((qrCodeImage.Width - logoImage.Width) / 2, (qrCodeImage.Height - logoImage.Height) / 2);
        ctx.DrawImage(logoImage, centerPosition, 1f); // 1f for full opacity
    });

    // Return the final image with QR code and logo
    return qrCodeImage.Clone();
    }

    private async Task<Image<Rgba32>> GetLogoFromS3Async(string bucketName, string logoKey)
    {
        using (var response = await _client.GetObjectAsync(bucketName, logoKey))
        {
            // Load the logo into a MemoryStream
            using (var ms = new MemoryStream())
            {
                await response.ResponseStream.CopyToAsync(ms);
                ms.Position = 0; // Reset stream position

                // Load image using ImageSharp
                return Image.Load<Rgba32>(ms); // Return an ImageSharp image directly
            }
        }
    }
}