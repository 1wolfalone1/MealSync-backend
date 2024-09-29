using System.Text;
using MealSync.Application.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MealSync.API.Attributes;

public class CacheAttribute : Attribute, IAsyncActionFilter
{
    private readonly int _timeToLiveInSeconds;

    public CacheAttribute(int timeToLiveInSeconds = 600)
    {
        _timeToLiveInSeconds = timeToLiveInSeconds;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Getting an instance of the `ICacheService` service from the dependency injection container
        var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();

        var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

        // Trying to find a cached response with key matches the cache key generated
        var cachedResponse = await cacheService.GetCachedResponseAsync(cacheKey);

        // If a cached response is found:
        if (!string.IsNullOrEmpty(cachedResponse))
        {
            // Setting ContentResult properties to match the cached response
            var contentResult = new ContentResult()
            {
                Content = cachedResponse,
                ContentType = "application/json",
                StatusCode = 200,
            };

            // Return the cached response for the current request to the client without executing the endpoint
            context.Result = contentResult;
            return;
        }

        // In case there is no cached response for the current request, invoke the next action in the pipeline (the endpoint)
        var executedContext = await next();

        // Cache that response for the future requests, if the executed action result is of type `OkObjectResult` object
        if (executedContext.Result is OkObjectResult response)
            await cacheService.SetCacheResponseAsync(cacheKey, response.Value,
                TimeSpan.FromSeconds(_timeToLiveInSeconds));
    }

    private static string GenerateCacheKeyFromRequest(HttpRequest request)
    {
        var cacheKey = new StringBuilder();
        cacheKey.Append($"{request.Path}");
        foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            cacheKey.Append($"|{key}-{value}");
        return cacheKey.ToString();
    }
}