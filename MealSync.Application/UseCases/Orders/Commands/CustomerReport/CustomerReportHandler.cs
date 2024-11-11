using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.CustomerReport;

public class CustomerReportHandler : ICommandHandler<CustomerReportCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerReportHandler> _logger;
    private readonly IMapper _mapper;

    public CustomerReportHandler(
        IOrderRepository orderRepository, IReportRepository reportRepository,
        ICurrentPrincipalService currentPrincipalService, IStorageService storageService,
        IUnitOfWork unitOfWork, ILogger<CustomerReportHandler> logger, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _reportRepository = reportRepository;
        _currentPrincipalService = currentPrincipalService;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(CustomerReportCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;

        var order = await _orderRepository.GetByIdAndCustomerId(request.OrderId, customerId).ConfigureAwait(false);
        if (order == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId });
        }
        else if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.FailDelivery)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_STATUS_FOR_CUSTOMER_REPORT.GetDescription());
        }
        else if (await _reportRepository.CheckExistedCustomerReport(request.OrderId, customerId).ConfigureAwait(false))
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_CUSTOMER_ALREADY_REPORT.GetDescription());
        }
        else
        {
            var now = DateTimeOffset.UtcNow;
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

            // BR: Report within 12 hours after the 'endTime'
            if (now >= endTime && now <= endTime.AddHours(12))
            {
                var imageUrls = new List<string>();
                foreach (var file in request.Images)
                {
                    var url = await _storageService.UploadFileAsync(file).ConfigureAwait(false);
                    imageUrls.Add(url);
                }

                var report = new Report
                {
                    OrderId = request.OrderId,
                    CustomerId = customerId,
                    Title = request.Title,
                    Content = request.Content,
                    ImageUrl = string.Join(",", imageUrls),
                    Status = ReportStatus.Pending,
                };
                order.Status = OrderStatus.IssueReported;
                order.ReasonIdentity = order.Status == OrderStatus.FailDelivery
                    ? OrderIdentityCode.ORDER_IDENTITY_DELIVERY_FAIL_REPORTED_BY_CUSTOMER.GetDescription()
                    : OrderIdentityCode.ORDER_IDENTITY_DELIVERED_REPORTED_BY_CUSTOMER.GetDescription();
                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                    await _reportRepository.AddAsync(report).ConfigureAwait(false);
                    _orderRepository.Update(order);
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
                throw new InvalidBusinessException(MessageCode.E_ORDER_CUSTOMER_REPORT_TIME_LIMIT.GetDescription());
            }
        }
    }
}