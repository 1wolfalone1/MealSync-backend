using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.WithdrawalRequests.Queries.GetDetailForAdmin;

public class GetDetailForAdminHandler : IQueryHandler<GetDetailForAdminQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;

    public GetDetailForAdminHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IWithdrawalRequestRepository withdrawalRequestRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
    }

    public async Task<Result<Result>> Handle(GetDetailForAdminQuery request, CancellationToken cancellationToken)
    {
        var withdrawalRequestDetail = await _withdrawalRequestRepository.GetDetailForAdmin(request.WithdrawalRequestId).ConfigureAwait(false);
        return Result.Success(withdrawalRequestDetail);
    }
}