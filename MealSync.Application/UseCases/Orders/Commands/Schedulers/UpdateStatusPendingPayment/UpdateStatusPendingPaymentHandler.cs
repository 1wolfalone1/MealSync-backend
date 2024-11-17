using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Schedulers.UpdateStatusPendingPayment;

public class UpdateStatusPendingPaymentHandler : ICommandHandler<UpdateStatusPendingPaymentCommand, Result>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateStatusPendingPaymentHandler> _logger;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IBatchHistoryRepository _batchHistoryRepository;

    public UpdateStatusPendingPaymentHandler(
        IPaymentRepository paymentRepository, IUnitOfWork unitOfWork, ILogger<UpdateStatusPendingPaymentHandler> logger,
        ISystemResourceRepository systemResourceRepository, IBatchHistoryRepository batchHistoryRepository)
    {
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _systemResourceRepository = systemResourceRepository;
        _batchHistoryRepository = batchHistoryRepository;
    }

    public async Task<Result<Result>> Handle(UpdateStatusPendingPaymentCommand request, CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var (intendedReceiveDate, startTime, endTime) = TimeFrameUtils.OrderTimeFrameForBatchProcess(TimeFrameUtils.GetCurrentDateInUTC7(), 0);
        var startBatchDateTime = TimeFrameUtils.GetCurrentDate();
        var endBatchDateTime = TimeFrameUtils.GetCurrentDate();
        _logger.LogInformation($"Update Status Pending Payment Batch Start At: {startBatchDateTime}  (Intended Receive Date: {intendedReceiveDate} - End Time: {endTime})");

        var payments = await _paymentRepository.GetPendingPaymentOrder(intendedReceiveDate, endTime).ConfigureAwait(false);

        if (payments.Count > 0)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                foreach (var payment in payments)
                {
                    payment.Order.Status = OrderStatus.Cancelled;
                    payment.Order.ReasonIdentity = OrderIdentityCode.ORDER_IDENTITY_CUSTOMER_CANCEL.GetDescription();
                    payment.Order.Reason = _systemResourceRepository.GetByResourceCode(MessageCode.I_PAYMENT_FAIL.GetDescription());
                }

                _paymentRepository.UpdateRange(payments);
                endBatchDateTime = TimeFrameUtils.GetCurrentDate();
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                errors.Add(e.Message);
            }

            try
            {
                await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
                var batchHistory = new BatchHistory
                {
                    BatchCode = BatchCodes.BatchCheduleMarkDeliveryFail,
                    Parameter = string.Empty,
                    TotalRecord = payments.Count,
                    ErrorLog = string.Join(", ", errors),
                    StartDateTime = startBatchDateTime,
                    EndDateTime = endBatchDateTime,
                };

                await _batchHistoryRepository.AddAsync(batchHistory).ConfigureAwait(false);
                await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                _logger.LogInformation($"Update Status Pending Payment Batch End At: {startBatchDateTime}");
                return errors.Count > 0 ? Result.Success("Run batch fail!") : Result.Success("Run batch success!");
            }
            catch (Exception e)
            {
                _unitOfWork.RollbackTransaction();
                _logger.LogError(e, e.Message);
                throw;
            }
        }

        return Result.Success("Run batch success!");
    }
}