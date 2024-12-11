using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Accounts.Queries.AdminManage.GetDetailAccount;

public class GetDetailAccountForAdminHandler : IQueryHandler<GetDetailAccountAdminQuery, Result>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;

    public GetDetailAccountForAdminHandler(
        ICustomerRepository customerRepository, ICurrentPrincipalService currentPrincipalService,
        IModeratorDormitoryRepository moderatorDormitoryRepository)
    {
        _customerRepository = customerRepository;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
    }

    public async Task<Result<Result>> Handle(GetDetailAccountAdminQuery request, CancellationToken cancellationToken)
    {
        var dormitories = _moderatorDormitoryRepository.Get().ToList();
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var accountDetail = await _customerRepository.GetCustomerDetail(dormitoryIds, request.AccountId).ConfigureAwait(false);
        if (accountDetail == default)
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }
        else
        {
            return Result.Success(accountDetail);
        }
    }
}