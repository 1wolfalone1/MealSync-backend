using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Models.Responses;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportForShopByFilter;

public class GetReportForShopByHandler : IQueryHandler<GetReportForShopByFilterQuery, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetReportForShopByHandler(IReportRepository reportRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _reportRepository = reportRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetReportForShopByFilterQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var data = await _reportRepository.GetByShopId(shopId, request.SearchValue, request.Status, request.DateFrom, request.DateTo, request.PageIndex, request.PageSize).ConfigureAwait(false);

        var now = DateTimeOffset.UtcNow;
        foreach (var report in data.Reports)
        {
            if (report.Reports.Count > 1)
            {
                report.IsAllowShopReply = false;
            }
            else
            {
                DateTime receiveDate;
                if (report.EndTime == 2400)
                {
                    receiveDate = new DateTime(
                            report.IntendedReceiveDate.Year,
                            report.IntendedReceiveDate.Month,
                            report.IntendedReceiveDate.Day,
                            0,
                            0,
                            0)
                        .AddDays(1);
                }
                else
                {
                    receiveDate = new DateTime(
                        report.IntendedReceiveDate.Year,
                        report.IntendedReceiveDate.Month,
                        report.IntendedReceiveDate.Day,
                        report.EndTime / 100,
                        report.EndTime % 100,
                        0);
                }

                var endTime = new DateTimeOffset(receiveDate, TimeSpan.FromHours(7));

                // BR: Reply report within 20 hours after the 'endTime'
                report.IsAllowShopReply = now >= endTime && now <= endTime.AddHours(20);
            }
        }

        var result = new PaginationResponse<ReportByOrderDto>(data.Reports, data.TotalCount, request.PageIndex, request.PageSize);

        return Result.Success(result);
    }
}