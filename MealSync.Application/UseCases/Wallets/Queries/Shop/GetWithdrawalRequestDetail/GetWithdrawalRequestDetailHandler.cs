using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Wallets.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Wallets.Queries.Shop.GetWithdrawalRequestDetail;

public class GetWithdrawalRequestDetailHandler : IQueryHandler<GetWithdrawalRequestDetailQuery, Result>
{
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetWithdrawalRequestDetailHandler(
        IWithdrawalRequestRepository withdrawalRequestRepository, IShopRepository shopRepository,
        ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _withdrawalRequestRepository = withdrawalRequestRepository;
        _shopRepository = shopRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetWithdrawalRequestDetailQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var shop = await _shopRepository.GetByAccountId(shopId).ConfigureAwait(false);

        var withdrawalRequest = await _withdrawalRequestRepository.GetDetailByIdAndWalletId(request.Id, shop.WalletId).ConfigureAwait(false);

        if (withdrawalRequest == default)
        {
            throw new InvalidBusinessException(MessageCode.E_WITHDRAWAL_NOT_FOUND.GetDescription(), new object[] { request.Id });
        }
        else
        {
            return Result.Success(_mapper.Map<WithdrawalRequestHistoryResponse>(withdrawalRequest));
        }
    }
}