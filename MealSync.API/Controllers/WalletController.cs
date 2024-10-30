using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Wallets.Commands.Shop.SendCodeWithdrawalRequest;
using MealSync.Application.UseCases.Wallets.Commands.Shop.VerifyWithdrawalRequest;
using MealSync.Application.UseCases.Wallets.Queries.Shop.GetWalletSummary;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MealSync.API.Controllers;

[Route(Endpoints.BASE)]
public class WalletController : BaseApiController
{
    [HttpGet(Endpoints.GET_SHOP_WALLET)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetShopWalletSummary()
    {
        return HandleResult(await Mediator.Send(new GetWalletSummaryQuery()).ConfigureAwait(false));
    }

    [HttpPost(Endpoints.WITHDRAWAL_REQUEST_SEND_VERIFY_CODE)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> WithdrawalRequestSendVerifyCode(SendCodeWithdrawalRequestCommand request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpPost(Endpoints.WITHDRAWAL_REQUEST)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> WithdrawalRequest(WithdrawalRequestCommand request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }
}