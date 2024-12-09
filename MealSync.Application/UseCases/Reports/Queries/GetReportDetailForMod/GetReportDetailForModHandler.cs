using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportDetailForMod;

public class GetReportDetailForModHandler : IQueryHandler<GetReportDetailForModQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetReportDetailForModHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IReportRepository reportRepository, IAccountRepository accountRepository,
        IOrderRepository orderRepository, IShopRepository shopRepository, IMapper mapper)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _reportRepository = reportRepository;
        _accountRepository = accountRepository;
        _orderRepository = orderRepository;
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetReportDetailForModQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var orderId = await _reportRepository.GetOrderIdByCustomerReportIdAndDormitoryIds(request.ReportId, dormitoryIds).ConfigureAwait(false);

        if (orderId == default || orderId == 0)
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }
        else
        {
            var reports = await _reportRepository.GetByOrderId(orderId.Value).ConfigureAwait(false);
            var customerReport = reports.First(r => r.CustomerId != default);
            var reportDetailShopWebResponses = new ReportDetailForModResponse();
            var order = _orderRepository.GetById(orderId.Value);

            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            DateTime receiveDateEndTime;
            if (order.EndTime == 2400)
            {
                receiveDateEndTime = new DateTime(
                        order.IntendedReceiveDate.Year,
                        order.IntendedReceiveDate.Month,
                        order.IntendedReceiveDate.Day,
                        0,
                        0,
                        0)
                    .AddDays(1);
            }
            else
            {
                receiveDateEndTime = new DateTime(
                    order.IntendedReceiveDate.Year,
                    order.IntendedReceiveDate.Month,
                    order.IntendedReceiveDate.Day,
                    order.EndTime / 100,
                    order.EndTime % 100,
                    0);
            }

            var endTime = new DateTimeOffset(receiveDateEndTime, TimeSpan.FromHours(7));
            if (customerReport.Status == ReportStatus.Pending && ((reports.Count > 1 && (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription() || now > endTime.AddHours(2))) || now > endTime.AddHours(20)))
            {
                reportDetailShopWebResponses.IsAllowAction = true;
            }

            reportDetailShopWebResponses.IsUnderReview = order.Status == OrderStatus.UnderReview;

            reportDetailShopWebResponses.CustomerInfo = _mapper.Map<ReportDetailForModResponse.CustomerInfoForModResponse>(_accountRepository.GetIncludeCustomerById(order.CustomerId));
            reportDetailShopWebResponses.ShopInfo = _mapper.Map<ReportDetailForModResponse.ShopInfoForModResponse>(_shopRepository.GetById(order.ShopId));
            reportDetailShopWebResponses.Reports = _mapper.Map<List<ReportDetailForModResponse.ReportResponse>>(reports);

            return Result.Success(reportDetailShopWebResponses);
        }
    }
}