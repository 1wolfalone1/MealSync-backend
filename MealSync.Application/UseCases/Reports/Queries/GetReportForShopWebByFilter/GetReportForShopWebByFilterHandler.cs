using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportForShopWebByFilter;

public class GetReportForShopWebByFilterHandler : IQueryHandler<GetReportForShopWebByFilterQuery, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentPrincipalService _currentPrincipalService;

    public GetReportForShopWebByFilterHandler(IReportRepository reportRepository, IMapper mapper, ICurrentPrincipalService currentPrincipalService)
    {
        _reportRepository = reportRepository;
        _mapper = mapper;
        _currentPrincipalService = currentPrincipalService;
    }

    public async Task<Result<Result>> Handle(GetReportForShopWebByFilterQuery request, CancellationToken cancellationToken)
    {
        var reportPaging = await _reportRepository.GetReportForShopWebFilter(_currentPrincipalService.CurrentPrincipalId.Value, request.SearchValue, request.Status, request.DateFrom, request.DateTo, request.PageIndex,
            request.PageSize).ConfigureAwait(false);

        var report = _mapper.Map<List<ReportForShopWebResponse>>(reportPaging.Reports);
        return Result.Success(new PaginationResponse<ReportForShopWebResponse>(report, reportPaging.TotalCount, request.PageIndex, request.PageSize));
    }
}