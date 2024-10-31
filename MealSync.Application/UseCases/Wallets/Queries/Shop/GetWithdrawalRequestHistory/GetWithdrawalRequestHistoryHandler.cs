using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Wallets.Models;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetWithdrawalRequestHistory;

public class GetWithdrawalRequestHistoryHandler : IQueryHandler<GetWithdrawalRequestHistoryQuery, Result>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetWithdrawalRequestHistoryHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository, IShopRepository shopRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetWithdrawalRequestHistoryQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = await _shopRepository.GetByAccountId(shopId).ConfigureAwait(false);

        var data = await _withdrawalRequestRepository.GetByFilter(
                shop.WalletId, request.Status, request.SearchValue,
                request.CreatedDate, request.PageIndex, request.PageSize)
            .ConfigureAwait(false);

        var result = new PaginationResponse<WithdrawalRequestHistoryResponse>(
            _mapper.Map<List<WithdrawalRequestHistoryResponse>>(data.WithdrawalRequests), data.TotalCount, request.PageIndex, request.PageSize);

        return Result.Success(result);
    }
}