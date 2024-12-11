using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Moderators.Models;

namespace MealSync.Application.UseCases.Moderators.Queries.GetActivityLogDetail;

public class GetActivityLogDetailHandler : IQueryHandler<GetActivityLogDetailQuery, Result>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IDapperService _dapperService;
    private readonly IMapper _mapper;
    private readonly IReportRepository _reportRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IWithdrawalRequestRepository _withdrawalRequestRepository;

    public GetActivityLogDetailHandler(IActivityLogRepository activityLogRepository, IDapperService dapperService, IMapper mapper, IReportRepository reportRepository, IAccountRepository accountRepository, IWithdrawalRequestRepository withdrawalRequestRepository)
    {
        _activityLogRepository = activityLogRepository;
        _dapperService = dapperService;
        _mapper = mapper;
        _reportRepository = reportRepository;
        _accountRepository = accountRepository;
        _withdrawalRequestRepository = withdrawalRequestRepository;
    }

    public async Task<Result<Result>> Handle(GetActivityLogDetailQuery request, CancellationToken cancellationToken)
    {
        var activityLog = _activityLogRepository.GetById(request.Id);
        var response = _mapper.Map<ModeratorActivityLogResponse>(activityLog);

        return Result.Success(response);
    }
}