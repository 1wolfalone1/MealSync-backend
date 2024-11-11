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
            DateTime receiveDate;
            if (order.EndTime == 2400)
            {
                receiveDate = new DateTime(
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
                receiveDate = new DateTime(
                    order.IntendedReceiveDate.Year,
                    order.IntendedReceiveDate.Month,
                    order.IntendedReceiveDate.Day,
                    order.EndTime / 100,
                    order.EndTime % 100,
                    0);
            }

            var endTime = new DateTimeOffset(receiveDate, TimeSpan.FromHours(7));

            // BR: Reply report within 20 hours after the 'endTime'
            if (now >= endTime && now <= endTime.AddHours(20))
            {
                var imageUrls = new List<string>();
                foreach (var file in request.Images)
                {
                    var url = await _storageService.UploadFileAsync(file).ConfigureAwait(false);
                    imageUrls.Add(url);
                }

                var report = new Report
                {
                    OrderId = order.Id,
                    ShopId = shopId,
                    Title = request.Title,
                    Content = request.Content,
                    ImageUrl = string.Join(",", imageUrls),
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