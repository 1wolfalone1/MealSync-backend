using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reports.Commands.UpdateReportStatusForMod;

public class UpdateReportStatusForModHandler : ICommandHandler<UpdateReportStatusForModCommand, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOrderRepository _orderRepository;

    public UpdateReportStatusForModHandler(
        IReportRepository reportRepository, IModeratorDormitoryRepository moderatorDormitoryRepository, ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository)
    {
        _reportRepository = reportRepository;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
    }

    public async Task<Result<Result>> Handle(UpdateReportStatusForModCommand request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var orderId = await _reportRepository.GetOrderIdByCustomerReportIdAndDormitoryIds(request.Id, dormitoryIds).ConfigureAwait(false);

        if (orderId == default || orderId == 0)
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }
        else
        {
            var order = await _orderRepository.GetOrderIncludePaymentById(orderId.Value).ConfigureAwait(false);
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

            var reports = await _reportRepository.GetByOrderId(orderId.Value).ConfigureAwait(false);
            var customerReport = reports.First(r => r.CustomerId != default);
            var shopReport = reports.Count > 1 ? reports.First(r => r.ShopId != default) : default;

            // BR: Mod report after shop reply report or now > time report limit(20h)
            if (customerReport.Status != ReportStatus.Pending)
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
            }
            else if (reports.Count > 1 || now > endTime.AddHours(20))
            {
                if (request.Status == ReportStatus.Approved)
                {
                    Payment payment = order.Payments.First(p => p.Type == PaymentTypes.Payment);
                    if (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER.GetDescription())
                    {
                        // Fail delivery
                        if (payment.PaymentMethods == PaymentMethods.COD)
                        {
                            // Payment COD
                        }
                        else
                        {
                            // Payment Online
                        }
                    }
                    else if (order.ReasonIdentity == OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription())
                    {
                        // Delivered
                        if (payment.PaymentMethods == PaymentMethods.COD)
                        {
                            // Payment COD
                        }
                        else
                        {
                            // Payment Online
                        }
                    }
                    else
                    {
                        throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
                    }
                }
                else if (request.Status == ReportStatus.Rejected)
                {

                }
                else
                {
                    throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
                }
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_MODERATOR_NOT_YET_PROCESSED_REPORT.GetDescription());
            }
        }
        throw new NotImplementedException();
    }
}