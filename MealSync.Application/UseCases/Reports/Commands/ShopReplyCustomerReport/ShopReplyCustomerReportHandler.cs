using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Reports.Commands.ShopReplyCustomerReport;

public class ShopReplyCustomerReportHandler : ICommandHandler<ShopReplyCustomerReportCommand, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShopReplyCustomerReportHandler> _logger;
    private readonly IMapper _mapper;

    public ShopReplyCustomerReportHandler(
        IReportRepository reportRepository, IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService,
        IStorageService storageService, IUnitOfWork unitOfWork, ILogger<ShopReplyCustomerReportHandler> logger, IMapper mapper)
    {
        _reportRepository = reportRepository;
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(ShopReplyCustomerReportCommand request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var orderId = await _reportRepository.GetOrderIdByIdAndShopId(request.ReplyReportId, shopId).ConfigureAwait(false);
        if (orderId == default)
        {
            throw new InvalidBusinessException(MessageCode.E_REPORT_NOT_FOUND.GetDescription(), new object[] { request.ReplyReportId });
        }
        else if (await _reportRepository.CheckExistedShopReplyReport(orderId.Value, shopId).ConfigureAwait(false))
        {
            throw new InvalidBusinessException(MessageCode.E_REPORT_SHOP_ALREADY_REPORT.GetDescription(), new object[] { request.ReplyReportId });
        }
        else
        {
            var now = DateTimeOffset.UtcNow;
            var order = _orderRepository.GetById(orderId)!;
            var receiveDateStartTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.StartTime / 100,
                order.StartTime % 100,
                0);
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

            var startTime = new DateTimeOffset(receiveDateStartTime, TimeSpan.FromHours(7));
            var endTime = new DateTimeOffset(receiveDateEndTime, TimeSpan.FromHours(7));

            // BR: Reply report within 20 hours after the 'endTime'
            if (now >= startTime && now <= endTime.AddHours(20))
            {
                var report = new Report
                {
                    OrderId = order.Id,
                    ShopId = shopId,
                    Title = request.Title,
                    Content = request.Content,
                    ImageUrl = string.Join(",", request.Images),
                    Status = ReportStatus.Pending,
                };
                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    await _reportRepository.AddAsync(report).ConfigureAwait(false);
                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                    return Result.Create(_mapper.Map<ReportDetailResponse>(report));
                }
                catch (Exception e)
                {
                    // Rollback when exception
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }
            }
            else
            {
                throw new InvalidBusinessException(MessageCode.E_REPORT_SHOP_REPORT_TIME_LIMIT.GetDescription());
            }
        }
    }
}