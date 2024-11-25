using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Dapper;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reports.Queries.GetAllReportForMod;

public class GetAllReportForModHandler : IQueryHandler<GetAllReportForModQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IDapperService _dapperService;

    public GetAllReportForModHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository, IDapperService dapperService)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _dapperService = dapperService;
    }

    public async Task<Result<Result>> Handle(GetAllReportForModQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        if (request.DormitoryId != default && request.DormitoryId > 0 && !dormitoryIds.Contains(request.DormitoryId.Value))
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }

        var now = TimeFrameUtils.GetCurrentDateInUTC7().DateTime;
        var reports = await _dapperService.SelectAsync<ReportManageDto>(QueryName.GetListReportForMod, new
        {
            Now = now,
            DormitoryIds = dormitoryIds,
            SearchValue = request.SearchValue,
            Status = request.Status,
            DormitoryId = request.DormitoryId,
            DateFrom = request.DateFrom,
            DateTo = request.DateTo,
            OrderBy = (int)request.OrderBy,
            Direction = (int)request.Direction,
            PageSize = request.PageSize,
            Offset = (request.PageIndex - 1) * request.PageSize,
        }).ConfigureAwait(false);

        var result = new PaginationResponse<ReportManageDto>(
            reports.ToList(), reports.Any() ? reports.First().TotalCount : 0, request.PageIndex, request.PageSize);

        return Result.Success(result);
    }
}