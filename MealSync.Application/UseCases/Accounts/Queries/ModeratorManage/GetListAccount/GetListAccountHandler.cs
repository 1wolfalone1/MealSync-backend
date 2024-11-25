using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Accounts.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Accounts.Queries.ModeratorManage.GetListAccount;

public class GetListAccountHandler : IQueryHandler<GetListAccountQuery, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;

    public GetListAccountHandler(
        ICustomerRepository customerRepository, ICurrentPrincipalService currentPrincipalService,
        IModeratorDormitoryRepository moderatorDormitoryRepository)
    {
        _customerRepository = customerRepository;
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
    }

    public async Task<Result<Result>> Handle(GetListAccountQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        if (request.DormitoryId != default && request.DormitoryId > 0 && !dormitoryIds.Contains(request.DormitoryId.Value))
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }

        var data = await _customerRepository.GetAllCustomer(
            dormitoryIds, request.SearchValue,
            request.DateFrom, request.DateTo,
            request.Status, request.DormitoryId, request.OrderBy,
            request.Direction, request.PageIndex,
            request.PageSize).ConfigureAwait(false);

        var result = new PaginationResponse<AccountForModManageDto>(
            data.Customers,
            data.TotalCount,
            request.PageIndex,
            request.PageSize);
        return Result.Success(result);
    }
}