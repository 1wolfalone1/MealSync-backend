using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;

namespace MealSync.Application.UseCases.CommissionConfigs.Queries.Get;

public class GetCommissionConfigHandler : IQueryHandler<GetCommissionConfigQuery, Result>
{
    private readonly ICommissionConfigRepository _commissionConfigRepository;

    public GetCommissionConfigHandler(ICommissionConfigRepository commissionConfigRepository)
    {
        _commissionConfigRepository = commissionConfigRepository;
    }

    public async Task<Result<Result>> Handle(GetCommissionConfigQuery request, CancellationToken cancellationToken)
    {
        var commissionConfig = _commissionConfigRepository.GetNewest();
        return Result.Success(new
        {
            Id = commissionConfig.Id,
            CommissionRate = commissionConfig.CommissionRate,
            CreatedDate = commissionConfig.CreatedDate,
            UpdatedDate = commissionConfig.UpdatedDate,
        });
    }
}