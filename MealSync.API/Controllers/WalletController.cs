using MealSync.API.Identites;
using MealSync.API.Shared;
using MealSync.Application.UseCases.Wallets.Commands.Shop.CancelWithdrawalRequest;
using MealSync.Application.UseCases.Wallets.Commands.Shop.SendCodeWithdrawalRequest;
using MealSync.Application.UseCases.Wallets.Commands.Shop.VerifyWithdrawalRequest;
using MealSync.Application.UseCases.Wallets.Queries.Shop.GetTransactionHistorys;
using MealSync.Application.UseCases.Wallets.Queries.Shop.GetWalletSummary;
using MealSync.Application.UseCases.Wallets.Queries.Shop.GetWithdrawalRequestDetail;
using MealSync.Application.UseCases.Wallets.Queries.Shop.GetWithdrawalRequestHistory;
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

    [HttpGet(Endpoints.WITHDRAWAL_REQUEST_HISTORY)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> WithdrawalRequestHistory([FromQuery] GetWithdrawalRequestHistoryQuery request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.WITHDRAWAL_REQUEST_DETAIL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> WithdrawalRequestDetail(long id)
    {
        return HandleResult(await Mediator.Send(new GetWithdrawalRequestDetailQuery { Id = id }).ConfigureAwait(false));
    }

    [HttpPut(Endpoints.WITHDRAWAL_REQUEST_CANCEL)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> CancelWithdrawalRequest(CancelWithdrawalRequestCommand request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }

    [HttpGet(Endpoints.GET_WALLET_TRANSACTION)]
    [Authorize(Roles = $"{IdentityConst.ShopClaimName}")]
    public async Task<IActionResult> GetWalletTransactionHistory([FromQuery] GetTransactionHistoryQuery request)
    {
        return HandleResult(await Mediator.Send(request).ConfigureAwait(false));
    }
}