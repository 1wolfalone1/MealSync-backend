using System.Net;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Services;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;
using Point = SixLabors.ImageSharp.Point;

namespace MealSync.Infrastructure.Services;

public class StorageService : IStorageService, IBaseService
{

    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly IAmazonS3 _client;
    private readonly IAmazonRekognition _rekognitionClient;
    private readonly HashSet<string> _foodCategories = new HashSet<string>
    {
        "Food", "Drink", "Beverage", "Meal", "Fruit", "Vegetable", "Dessert"
    };

    public StorageService(ILogger<StorageService> logger, IConfiguration configuration, IAmazonS3 client, IAmazonRekognition rekognitionClient)
    {
        _logger = logger;
        _configuration = configuration;
        _client = client;
        _rekognitionClient = rekognitionClient;
    }

    public async Task<string> UploadFileAsync(IFormFile file, bool isCheckFoodDrink)
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
            await CheckImageViolation(ms, isCheckFoodDrink).ConfigureAwait(false);
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

    private async Task CheckImageViolation(MemoryStream ms, bool isCheckFoodDrink)
    {
        var responseDetectModeration = await _rekognitionClient.DetectModerationLabelsAsync(new DetectModerationLabelsRequest
        {
            Image = new Amazon.Rekognition.Model.Image
            {
                Bytes = new MemoryStream(ms.ToArray()),
            },
            MinConfidence = 50,
        }).ConfigureAwait(false);

        if (responseDetectModeration.ModerationLabels.Count != 0)
        {
            var categorizedViolations = new Dictionary<string, List<Violation>>();
            foreach (var label in responseDetectModeration.ModerationLabels)
            {
                var parentName = string.IsNullOrEmpty(label.ParentName) ? "Other" : label.ParentName;

                if (!categorizedViolations.ContainsKey(parentName))
                {
                    categorizedViolations[parentName] = new List<Violation>();
                }

                categorizedViolations[parentName].Add(new Violation
                {
                    Name = label.Name,
                    Confidence = label.Confidence,
                });
            }

            throw new InvalidBusinessException(MessageCode.E_VIOLATION_IMAGE.GetDescription());
        }

        if (isCheckFoodDrink)
        {
            var responseDetectLabels = await _rekognitionClient.DetectLabelsAsync(new DetectLabelsRequest
            {
                Image = new Amazon.Rekognition.Model.Image
                {
                    Bytes = new MemoryStream(ms.ToArray()),
                },
                MinConfidence = 70,
                MaxLabels = 50, // Adjust as needed
            }).ConfigureAwait(false);

            var isFood = false;
            foreach (var label in responseDetectLabels.Labels)
            {
                isFood = _foodCategories.Contains(label.Name) ||
                         label.Parents.Any(p => _foodCategories.Contains(p.Name));
                if (isFood)
                {
                    break;
                }
            }

            if (!isFood)
            {
                throw new InvalidBusinessException(MessageCode.E_VIOLATION_IMAGE_NOT_FOOD_DRINK.GetDescription());
            }
        }
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

        // Step 1: Generate the QR code and retrieve it as a byte array
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrData);
        byte[] qrCodeBytes = qrCode.GetGraphic(20);

        // Step 2: Load the QR code byte array into an ImageSharp Image<Rgba32>
        using var qrCodeImage = Image.Load<Rgba32>(qrCodeBytes);

        // Step 3: Retrieve the logo from S3 and load it as an ImageSharp image
        var logoImage = await GetLogoFromS3Async(bucketName, logoKey);

        // Resize the logo to fit in the center of the QR code
        int logoSize = qrCodeImage.Width / 5; // Adjust size as needed
        logoImage.Mutate(x => x.Resize(logoSize, logoSize));

        // Center the logo on the QR code
        var centerPosition = new Point(
            (qrCodeImage.Width - logoImage.Width) / 2,
            (qrCodeImage.Height - logoImage.Height) / 2
        );
        qrCodeImage.Mutate(ctx => ctx.DrawImage(logoImage, centerPosition, 1f));

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

    public class Violation
    {
        public string Name { get; set; }

        public float Confidence { get; set; }
    }
}