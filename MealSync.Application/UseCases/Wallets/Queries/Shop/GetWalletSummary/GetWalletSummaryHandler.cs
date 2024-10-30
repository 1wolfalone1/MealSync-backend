using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Wallets.Models;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetWalletSummary;

public class GetWalletSummaryHandler : IQueryHandler<GetWalletSummaryQuery, Result>
{
    private readonly IWalletRepository _walletRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetWalletSummaryHandler(
        IWalletRepository walletRepository, IShopRepository shopRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _walletRepository = walletRepository;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetWalletSummaryQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = await _shopRepository.GetByAccountId(shopId).ConfigureAwait(false);
        var wallet = _walletRepository.GetById(shop.WalletId);
        return Result.Success(_mapper.Map<WalletSummaryResponse>(wallet));
    }
}