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
}