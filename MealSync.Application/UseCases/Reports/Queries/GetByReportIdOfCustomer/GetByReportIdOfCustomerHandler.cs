using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reports.Queries.GetByReportIdOfCustomer;

public class GetByReportIdOfCustomerHandler : IQueryHandler<GetByReportIdOfCustomerQuery, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IMapper _mapper;

    public GetByReportIdOfCustomerHandler(IReportRepository reportRepository, ICurrentPrincipalService currentPrincipalService, IMapper mapper)
    {
        _reportRepository = reportRepository;
        _currentPrincipalService = currentPrincipalService;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetByReportIdOfCustomerQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var orderId = await _reportRepository.GetOrderIdByIdAndShopId(request.CustomerReportId, shopId).ConfigureAwait(false);
        if (orderId.HasValue && orderId.Value > 0)
        {
            var reports = await _reportRepository.GetByOrderId(orderId.Value).ConfigureAwait(false);
            return Result.Create(_mapper.Map<List<ReportDetailResponse>>(reports));
        }
        else
        {
            throw new InvalidBusinessException(MessageCode.E_REPORT_NOT_FOUND.GetDescription(), new object[] { request.CustomerReportId });
        }
    }
}