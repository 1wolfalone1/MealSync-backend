using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Wallets.Models;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetTransactionHistorys;

public class GetTransactionHistoryHandler : IQueryHandler<GetTransactionHistoryQuery, Result>
{
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetTransactionHistoryHandler(IWalletTransactionRepository walletTransactionRepository, ICurrentPrincipalService currentPrincipalService, IShopRepository shopRepository, IMapper mapper)
    {
        _walletTransactionRepository = walletTransactionRepository;
        _currentPrincipalService = currentPrincipalService;
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetTransactionHistoryQuery request, CancellationToken cancellationToken)
    {
        var shop = _shopRepository.GetById(_currentPrincipalService.CurrentPrincipalId.Value);
        var walletTransactionPage = _walletTransactionRepository.GetShopTransactionHistory(shop.WalletId, request.PageIndex, request.PageSize);
        var response = new PaginationResponse<WalletTransactionResponse>(_mapper.Map<List<WalletTransactionResponse>>(walletTransactionPage.WalletTransactions), walletTransactionPage.TotalCount, request.PageIndex,
            request.PageSize);
        return Result.Success(response);
    }
}