using System.Text;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Application.Common.Services.Payments.VnPay.Shared;
using MealSync.Application.Shared;
using MealSync.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.UpdatePaymentStatusIPN;

public class UpdatePaymentStatusIPNHandler : ICommandHandler<UpdatePaymentStatusIPNCommand, VnPayIPNResponse>
{
    private readonly IVnPayPaymentService _paymentService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<UpdatePaymentStatusIPNHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePaymentStatusIPNHandler(IVnPayPaymentService paymentService, IPaymentRepository paymentRepository, ILogger<UpdatePaymentStatusIPNHandler> logger, IUnitOfWork unitOfWork)
    {
        _paymentService = paymentService;
        _paymentRepository = paymentRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VnPayIPNResponse>> Handle(UpdatePaymentStatusIPNCommand request, CancellationToken cancellationToken)
    {
        var parseOrderId = Int64.TryParse(request.Query[VnPayRequestParam.VNP_TXN_REF], out var orderId);
        if (parseOrderId)
        {
            var payment = await _paymentRepository.GetOldestByOrderId(orderId).ConfigureAwait(false);
            var response = await _paymentService.GetIPN(request.Query, payment).ConfigureAwait(false);
            if (response.RspCode == ((int)VnPayIPNResponseCode.CODE_00).ToString("D2"))
            {
                if (response.Message == "Confirm Success")
                {
                    payment.Status = PaymentStatus.PaidSuccess;
                    payment.PaymentThirdPartyId = request.Query[VnPayRequestParam.VNP_TRANSACTION_NO];
                    payment.PaymentThirdPartyContent = ConvertQueryCollectionToString(request.Query);
                }
                else
                {
                    payment.Status = PaymentStatus.PaidFail;
                    response.Message = "Confirm Success";
                }

                try
                {
                    // Begin transaction
                    await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                    _paymentRepository.Update(payment);

                    await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // Rollback when exception
                    _unitOfWork.RollbackTransaction();
                    _logger.LogError(e, e.Message);
                    throw new("Internal Server Error");
                }
            }
            return Result.Success(response);
        }
        else
        {
            var response = new VnPayIPNResponse();
            response.RspCode = ((int)VnPayIPNResponseCode.CODE_02).ToString("D2");
            response.Message = VnPayIPNResponseCode.CODE_02.GetDescription();
            return response;
        }
    }

    private string ConvertQueryCollectionToString(IQueryCollection query)
    {
        var stringBuilder = new StringBuilder();

        foreach (var key in query.Keys)
        {
            var values = query[key];  // IQueryCollection may contain multiple values for a key.
            foreach (var value in values)
            {
                stringBuilder.Append($"{key}={value}&");
            }
        }

        // Remove the trailing '&' if it exists.
        if (stringBuilder.Length > 0)
        {
            stringBuilder.Length--;
        }

        return stringBuilder.ToString();
    }
}