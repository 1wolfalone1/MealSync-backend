using System.Net;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text.RegularExpressions;
using MealSync.API.Shared;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.AspNetCore.Authentication;

namespace MealSync.API.Middleware;

public class CheckBanningMiddleware
{
    private readonly RequestDelegate _next;
    private readonly List<string> _whiteListedUris;

    public CheckBanningMiddleware(RequestDelegate next)
    {
        _next = next;

        // Populate the whitelist using enum descriptions
        _whiteListedUris = EnumExtensions.GetAllDescriptions<WhiteEndpointsAcceptBanning>();
    }

    public async Task InvokeAsync(HttpContext context, ICustomerRepository customerRepository, IShopRepository shopRepository)
    {
        context.Request.EnableBuffering();

        var identity = context?.User.Identity as ClaimsIdentity;
        if (identity == null || !identity.IsAuthenticated)
        {
            await _next(context);
            return;
        }

        var claims = identity.Claims;
        var role = claims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value ?? null;
        var accountId = claims.FirstOrDefault(o => o.Type == ClaimTypes.Sid)?.Value ?? null;
        if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(accountId) ||
            role != Roles.Customer.GetDescription() && role != Roles.ShopOwner.GetDescription())
        {
            await _next(context);
            return;
        }

        if (role == Roles.Customer.GetDescription())
        {
            var customer = customerRepository.GetById(long.Parse(accountId));
            if (customer == null)
            {
                throw new InvalidBusinessException(MessageCode.E_TOKEN_NOT_VALID.GetDescription(), HttpStatusCode.Unauthorized);
            }
            ValidateRequest(customer.Status, context.Request.Path.Value, context.Request.Method);
        }
        else if (role == Roles.ShopOwner.GetDescription())
        {
            var shop = shopRepository.GetById(long.Parse(accountId));
            if (shop == null)
            {
                throw new InvalidBusinessException(MessageCode.E_TOKEN_NOT_VALID.GetDescription(), HttpStatusCode.Unauthorized);
            }
            ValidateRequest(shop.Status, context.Request.Path.Value, context.Request.Method);
        }

        await _next(context);
    }

    private void ValidateRequest(CustomerStatus accountStatus, string requestUri, string httpMethod)
    {
        if (accountStatus == CustomerStatus.Banning && IsHttpMethodAllowed(httpMethod))
        {
            if (!_whiteListedUris.Any(uri => IsUriMatch(uri, requestUri)))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_IN_BANNING.GetDescription(), HttpStatusCode.Forbidden);
            }
        }

        if (accountStatus == CustomerStatus.Banned)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_BANNED.GetDescription(), HttpStatusCode.Forbidden);
        }
    }

    private void ValidateRequest(ShopStatus accountStatus, string requestUri, string httpMethod)
    {
        if (accountStatus == ShopStatus.Banning && IsHttpMethodAllowed(httpMethod))
        {
            if (!_whiteListedUris.Any(uri => IsUriMatch(uri, requestUri)))
            {
                throw new InvalidBusinessException(MessageCode.E_ACCOUNT_IN_BANNING.GetDescription(), HttpStatusCode.Forbidden);
            }
        }

        if (accountStatus == ShopStatus.Banned)
        {
            throw new InvalidBusinessException(MessageCode.E_ACCOUNT_BANNED.GetDescription(), HttpStatusCode.Forbidden);
        }
    }

    private bool IsHttpMethodAllowed(string method)
    {
        return method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("DELETE", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsUriMatch(string template, string requestUri)
    {
        // Replace placeholder syntax like "{id:long}" for matching
        var regex = "^" + Regex.Escape(template).Replace("{id:long}", "d+") + "$";
        return Regex.IsMatch(requestUri, regex);
    }
}