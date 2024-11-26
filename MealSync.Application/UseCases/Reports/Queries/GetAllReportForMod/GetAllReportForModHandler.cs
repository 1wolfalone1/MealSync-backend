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
        var isAllowAction = false;
        var isAllStatus = false;
        var isUnderReview = false;
        var statusList = new List<ReportStatus>();

        if (request.DormitoryId != default && request.DormitoryId > 0 && !dormitoryIds.Contains(request.DormitoryId.Value))
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }

        if (request.Status == GetAllReportForModQuery.FilterReportStatus.PendingNotAllowAction)
        {
            statusList.Add(ReportStatus.Pending);
            isAllowAction = false;
        }
        else if (request.Status == GetAllReportForModQuery.FilterReportStatus.PendingAllowAction)
        {
            statusList.Add(ReportStatus.Pending);
            isAllowAction = true;
        }
        else if (request.Status == GetAllReportForModQuery.FilterReportStatus.UnderReview)
        {
            statusList.Add(ReportStatus.Pending);
            isAllowAction = true;
            isUnderReview = true;
        }
        else if (request.Status == GetAllReportForModQuery.FilterReportStatus.Rejected)
        {
            statusList.Add(ReportStatus.Rejected);
            isAllowAction = false;
        }
        else if (request.Status == GetAllReportForModQuery.FilterReportStatus.Approved)
        {
            statusList.Add(ReportStatus.Approved);
            isAllowAction = false;
        }
        else
        {
            // All
            isAllStatus = true;
            statusList.Add(ReportStatus.Pending);
            statusList.Add(ReportStatus.Approved);
            statusList.Add(ReportStatus.Rejected);
        }

        var now = TimeFrameUtils.GetCurrentDateInUTC7().DateTime;
        var reports = await _dapperService.SelectAsync<ReportManageDto>(QueryName.GetListReportForMod, new
        {
            Now = now,
            DormitoryIds = dormitoryIds,
            SearchValue = request.SearchValue,
            IsAllowAction = isAllowAction,
            IsUnderReview = isUnderReview,
            IsAllStatus = isAllStatus,
            StatusList = statusList,
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