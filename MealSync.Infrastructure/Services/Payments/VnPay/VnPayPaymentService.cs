using System.Net;
using System.Text.Json;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Services.Payments.VnPay.Models;
using MealSync.Application.Common.Services.Payments.VnPay.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Infrastructure.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MealSync.Infrastructure.Services.Payments.VnPay;

public class VnPayPaymentService : IVnPayPaymentService, IBaseService
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly VnPaySetting _vnPaySetting;
    private readonly ILogger<VnPayPaymentService> _logger;

    private const string PAYMENT = "pay";
    private const string REFUND = "refund";
    private const string REFUND_TRANSACTION_TYPE_FULL = "02";
    private const string DATE_FORMAT = "yyyyMMddHHmmss";
    private const string CURRENCY_CODE = "VND";
    private const string LOCALE = "vn";
    private const string ORDER_TYPE = "180000"; // Ẩm thực
    private const string VN_BANK = "VNBANK";

    public VnPayPaymentService(IHttpContextAccessor contextAccessor, VnPaySetting vnPaySetting, ILogger<VnPayPaymentService> logger)
    {
        _contextAccessor = contextAccessor;
        _vnPaySetting = vnPaySetting;
        _logger = logger;
    }

    public async Task<string> CreatePaymentOrderUrl(Payment payment)
    {
        HttpContext? context = _contextAccessor.HttpContext;

        // if (context == null)
        // {
        //     throw new Exception("Http Context not found");
        // }

        var vnPay = new VnPayLibrary();

        vnPay.AddRequestData(VnPayRequestParam.VNP_VERSION, VnPayLibrary.VERSION);
        vnPay.AddRequestData(VnPayRequestParam.VNP_COMMAND, PAYMENT);
        vnPay.AddRequestData(VnPayRequestParam.VNP_TMN_CODE, _vnPaySetting.TmpCode);
        vnPay.AddRequestData(VnPayRequestParam.VNP_AMOUNT, (payment.Amount * 100).ToString());
        vnPay.AddRequestData(VnPayRequestParam.VNP_BANK_CODE, VN_BANK);
        vnPay.AddRequestData(VnPayRequestParam.VNP_CREATE_DATE, payment.CreatedDate.ToOffset(TimeSpan.FromHours(7)).ToString(DATE_FORMAT));
        vnPay.AddRequestData(VnPayRequestParam.VNP_CURR_CODE, CURRENCY_CODE);
        vnPay.AddRequestData(VnPayRequestParam.VNP_IP_ADDRESS, Utils.GetIpAddress(context));
        vnPay.AddRequestData(VnPayRequestParam.VNP_LOCALE, LOCALE);
        vnPay.AddRequestData(VnPayRequestParam.VNP_ORDER_INFO, VnPayPaymentType.ORDER_PAYMENT + " Thanh toan don hang: " + payment.OrderId);
        vnPay.AddRequestData(VnPayRequestParam.VNP_ORDER_TYPE, ORDER_TYPE);
        vnPay.AddRequestData(VnPayRequestParam.VNP_RETURN_URL, _vnPaySetting.ReturnUrl);
        vnPay.AddRequestData(VnPayRequestParam.VNP_EXPIRE_DATE, payment.CreatedDate.ToOffset(TimeSpan.FromHours(7)).AddMinutes(10).ToString(DATE_FORMAT));
        vnPay.AddRequestData(VnPayRequestParam.VNP_TXN_REF, payment.Id.ToString());

        return vnPay.CreateRequestUrl(_vnPaySetting.PaymentUrl, _vnPaySetting.HashSecret);
    }

    public async Task<string> CreatePaymentDepositUrl(Deposit deposit)
    {
        HttpContext? context = _contextAccessor.HttpContext;

        var vnPay = new VnPayLibrary();

        vnPay.AddRequestData(VnPayRequestParam.VNP_VERSION, VnPayLibrary.VERSION);
        vnPay.AddRequestData(VnPayRequestParam.VNP_COMMAND, PAYMENT);
        vnPay.AddRequestData(VnPayRequestParam.VNP_TMN_CODE, _vnPaySetting.TmpCode);
        vnPay.AddRequestData(VnPayRequestParam.VNP_AMOUNT, (deposit.Amount * 100).ToString());
        vnPay.AddRequestData(VnPayRequestParam.VNP_BANK_CODE, VN_BANK);
        vnPay.AddRequestData(VnPayRequestParam.VNP_CREATE_DATE, deposit.CreatedDate.ToOffset(TimeSpan.FromHours(7)).ToString(DATE_FORMAT));
        vnPay.AddRequestData(VnPayRequestParam.VNP_CURR_CODE, CURRENCY_CODE);
        vnPay.AddRequestData(VnPayRequestParam.VNP_IP_ADDRESS, Utils.GetIpAddress(context));
        vnPay.AddRequestData(VnPayRequestParam.VNP_LOCALE, LOCALE);
        vnPay.AddRequestData(VnPayRequestParam.VNP_ORDER_INFO, VnPayPaymentType.DEPOSIT + " Nap tien vao vi: " + deposit.WalletId);
        vnPay.AddRequestData(VnPayRequestParam.VNP_ORDER_TYPE, ORDER_TYPE);
        vnPay.AddRequestData(VnPayRequestParam.VNP_RETURN_URL, _vnPaySetting.ReturnUrl);
        vnPay.AddRequestData(VnPayRequestParam.VNP_EXPIRE_DATE, deposit.CreatedDate.ToOffset(TimeSpan.FromHours(7)).AddMinutes(10).ToString(DATE_FORMAT));
        vnPay.AddRequestData(VnPayRequestParam.VNP_TXN_REF, deposit.Id.ToString());

        return vnPay.CreateRequestUrl(_vnPaySetting.PaymentUrl, _vnPaySetting.HashSecret);
    }

    public async Task<VnPayRefundResponse> CreateRefund(Payment payment)
    {
        HttpContext? context = _contextAccessor.HttpContext;

        // if (context == null)
        // {
        //     throw new Exception("Http Context not found");
        // }

        var vnpHashSecret = _vnPaySetting.HashSecret;
        var vnpTmnCode = _vnPaySetting.TmpCode;
        var vnpRequestId = DateTime.Now.Ticks;
        var vnpVersion = VnPayLibrary.VERSION;
        var vnpCommand = REFUND;
        var vnpTransactionType = REFUND_TRANSACTION_TYPE_FULL;
        var vnpAmount = Convert.ToInt64(payment.Amount) * 100;
        var vnpTxnRef = payment.Id;
        var vnpOrderInfo = "Hoan tien giao dich don hang: " + payment.OrderId;
        var vnPayTransactionNo = Convert.ToInt64(payment.PaymentThirdPartyId);
        var vnpTransactionDate = payment.CreatedDate.ToOffset(TimeSpan.FromHours(7)).ToString(DATE_FORMAT);
        var vnpCreateDate = DateTime.Now.ToString(DATE_FORMAT);
        var vnpCreateBy = "Admin";
        var ipAddress = Utils.GetIpAddress(context);
        var signData = vnpRequestId + "|" + vnpVersion + "|" + vnpCommand + "|" + vnpTmnCode +
                       "|" + vnpTransactionType + "|" + vnpTxnRef + "|" + vnpAmount +
                       "|" + vnPayTransactionNo + "|" + vnpTransactionDate + "|" + vnpCreateBy +
                       "|" + vnpCreateDate + "|" + ipAddress + "|" + vnpOrderInfo;
        var vnpSecureHash = Utils.HmacSHA512(vnpHashSecret, signData);
        var rfData = new
        {
            vnp_RequestId = vnpRequestId,
            vnp_Version = vnpVersion,
            vnp_Command = vnpCommand,
            vnp_TmnCode = vnpTmnCode,
            vnp_TransactionType = vnpTransactionType,
            vnp_TxnRef = vnpTxnRef,
            vnp_Amount = vnpAmount,
            vnp_OrderInfo = vnpOrderInfo,
            vnp_TransactionNo = vnPayTransactionNo,
            vnp_TransactionDate = vnpTransactionDate,
            vnp_CreateBy = vnpCreateBy,
            vnp_CreateDate = vnpCreateDate,
            vnp_IpAddr = ipAddress,
            vnp_SecureHash = vnpSecureHash,
        };
        var jsonData = JsonSerializer.Serialize(rfData);
        var httpWebRequest = (HttpWebRequest)WebRequest.Create(_vnPaySetting.RefundUrl);

        httpWebRequest.ContentType = "application/json";
        httpWebRequest.Method = "POST";

        using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        {
            streamWriter.Write(jsonData);
        }

        using (var httpResponse = (HttpWebResponse)await httpWebRequest.GetResponseAsync().ConfigureAwait(false))
        {
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var responseText = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                var refundResponse = JsonSerializer.Deserialize<VnPayRefundResponse>(responseText);
                return refundResponse;
            }
        }
    }

    public async Task<VnPayIPNResponse> GetIPNPaymentOrder(IQueryCollection queryParams, Payment payment)
    {
        VnPayIPNResponse response = new VnPayIPNResponse();

        if (queryParams.Count > 0)
        {
            string vnpHashSecret = _vnPaySetting.HashSecret;
            VnPayLibrary vnPay = new VnPayLibrary();

            // Iterate over query parameters
            foreach (string key in queryParams.Keys)
            {
                // Add only VNPAY-related parameters that start with "vnp_"
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, queryParams[key]);
                }
            }

            // Extract values from vnPay object
            long paymentId = Convert.ToInt64(vnPay.GetResponseData(VnPayRequestParam.VNP_TXN_REF));
            long vnpAmount = Convert.ToInt64(vnPay.GetResponseData(VnPayRequestParam.VNP_AMOUNT)) / 100; // Convert to original amount
            long vnpTranId = Convert.ToInt64(vnPay.GetResponseData(VnPayRequestParam.VNP_TRANSACTION_NO));
            string vnpResponseCode = vnPay.GetResponseData(VnPayRequestParam.VNP_RESPONSE_CODE);
            string vnpTransactionStatus = vnPay.GetResponseData(VnPayRequestParam.VNP_TRANSACTION_STATUS);
            string vnpSecureHash = queryParams[VnPayRequestParam.VNP_SECURE_HASH];

            // Validate the signature
            bool checkSignature = vnPay.ValidateSignature(vnpSecureHash, vnpHashSecret);

            if (checkSignature)
            {
                if (payment.Id == paymentId)
                {
                    if (Convert.ToInt64(payment.Amount) == vnpAmount)
                    {
                        if (payment.Status == PaymentStatus.Pending || payment.Status == PaymentStatus.PaidFail)
                        {
                            if (vnpResponseCode == ((int)VnPayTransactionStatus.CODE_00).ToString("D2")
                                && vnpTransactionStatus == ((int)VnPayTransactionStatus.CODE_00).ToString("D2"))
                            {
                                // Payment success
                                response.RspCode = ((int)VnPayIPNResponseCode.CODE_00).ToString("D2");
                                response.Message = "Confirm Success";
                                _logger.LogInformation("VNPAY: Thanh toan thanh cong, OrderId={0}, VnPay TranId={1}", payment.OrderId, vnpTranId);
                            }
                            else
                            {
                                // Payment failed
                                response.RspCode = ((int)VnPayIPNResponseCode.CODE_00).ToString("D2");
                                response.Message = "Payment Failed";
                                _logger.LogWarning("VNPAY: Thanh toan loi, OrderId={0}, VnPay TranId={1}", payment.OrderId, vnpTranId);
                            }
                        }
                        else
                        {
                            // Payment already confirmed
                            response.RspCode = ((int)VnPayIPNResponseCode.CODE_02).ToString("D2");
                            response.Message = VnPayIPNResponseCode.CODE_02.GetDescription();
                            _logger.LogWarning("VNPAY: Payment already confirmed, OrderId={0}, VnPay TranId={1}", payment.OrderId, vnpTranId);
                        }
                    }
                    else
                    {
                        // Invalid amount
                        response.RspCode = ((int)VnPayIPNResponseCode.CODE_04).ToString("D2");
                        response.Message = VnPayIPNResponseCode.CODE_04.GetDescription();
                        _logger.LogWarning("VNPAY: Payment already confirmed, OrderId={0}, VnPay TranId={1}", payment.OrderId, vnpTranId);
                    }
                }
                else
                {
                    // Order not found
                    response.RspCode = ((int)VnPayIPNResponseCode.CODE_01).ToString("D2");
                    response.Message = VnPayIPNResponseCode.CODE_01.GetDescription();
                    _logger.LogWarning("VNPAY: Order not found, OrderId={0}, VnPay TranId={1}", payment.OrderId, vnpTranId);
                }
            }
            else
            {
                // Invalid signature
                response.RspCode = ((int)VnPayIPNResponseCode.CODE_97).ToString("D2");
                response.Message = VnPayIPNResponseCode.CODE_97.GetDescription();
                _logger.LogWarning("VNPAY: Invalid signature");
            }
        }
        else
        {
            // No query parameters provided
            response.RspCode = ((int)VnPayIPNResponseCode.CODE_99).ToString("D2");
            response.Message = VnPayIPNResponseCode.CODE_99.GetDescription();
            _logger.LogWarning("VNPAY: No query parameters provided");
        }

        return response;
    }

    public async Task<VnPayIPNResponse> GetIPNDeposit(IQueryCollection queryParams, Deposit deposit)
    {
        VnPayIPNResponse response = new VnPayIPNResponse();

        if (queryParams.Count > 0)
        {
            string vnpHashSecret = _vnPaySetting.HashSecret;
            VnPayLibrary vnPay = new VnPayLibrary();

            // Iterate over query parameters
            foreach (string key in queryParams.Keys)
            {
                // Add only VNPAY-related parameters that start with "vnp_"
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, queryParams[key]);
                }
            }

            // Extract values from vnPay object
            long depositId = Convert.ToInt64(vnPay.GetResponseData(VnPayRequestParam.VNP_TXN_REF));
            long vnpAmount = Convert.ToInt64(vnPay.GetResponseData(VnPayRequestParam.VNP_AMOUNT)) / 100; // Convert to original amount
            long vnpTranId = Convert.ToInt64(vnPay.GetResponseData(VnPayRequestParam.VNP_TRANSACTION_NO));
            string vnpResponseCode = vnPay.GetResponseData(VnPayRequestParam.VNP_RESPONSE_CODE);
            string vnpTransactionStatus = vnPay.GetResponseData(VnPayRequestParam.VNP_TRANSACTION_STATUS);
            string vnpSecureHash = queryParams[VnPayRequestParam.VNP_SECURE_HASH];

            // Validate the signature
            bool checkSignature = vnPay.ValidateSignature(vnpSecureHash, vnpHashSecret);

            if (checkSignature)
            {
                if (deposit.Id == depositId)
                {
                    if (Convert.ToInt64(deposit.Amount) == vnpAmount)
                    {
                        if (deposit.Status == DepositStatus.Pending)
                        {
                            if (vnpResponseCode == ((int)VnPayTransactionStatus.CODE_00).ToString("D2")
                                && vnpTransactionStatus == ((int)VnPayTransactionStatus.CODE_00).ToString("D2"))
                            {
                                // Payment success
                                response.RspCode = ((int)VnPayIPNResponseCode.CODE_00).ToString("D2");
                                response.Message = "Confirm Success";
                                _logger.LogInformation("VNPAY: Thanh toan thanh cong, DepositId={0}, VnPay TranId={1}", deposit.Id, vnpTranId);
                            }
                            else
                            {
                                // Payment failed
                                response.RspCode = ((int)VnPayIPNResponseCode.CODE_00).ToString("D2");
                                response.Message = "Payment Failed";
                                _logger.LogWarning("VNPAY: Thanh toan loi, DepositId={0}, VnPay TranId={1}", deposit.Id, vnpTranId);
                            }
                        }
                        else
                        {
                            // Payment already confirmed
                            response.RspCode = ((int)VnPayIPNResponseCode.CODE_02).ToString("D2");
                            response.Message = VnPayIPNResponseCode.CODE_02.GetDescription();
                            _logger.LogWarning("VNPAY: Payment already confirmed, DepositId={0}, VnPay TranId={1}", deposit.Id, vnpTranId);
                        }
                    }
                    else
                    {
                        // Invalid amount
                        response.RspCode = ((int)VnPayIPNResponseCode.CODE_04).ToString("D2");
                        response.Message = VnPayIPNResponseCode.CODE_04.GetDescription();
                        _logger.LogWarning("VNPAY: Payment already confirmed, DepositId={0}, VnPay TranId={1}", deposit.Id, vnpTranId);
                    }
                }
                else
                {
                    // Order not found
                    response.RspCode = ((int)VnPayIPNResponseCode.CODE_01).ToString("D2");
                    response.Message = VnPayIPNResponseCode.CODE_01.GetDescription();
                    _logger.LogWarning("VNPAY: Order not found, DepositId={0}, VnPay TranId={1}", deposit.Id, vnpTranId);
                }
            }
            else
            {
                // Invalid signature
                response.RspCode = ((int)VnPayIPNResponseCode.CODE_97).ToString("D2");
                response.Message = VnPayIPNResponseCode.CODE_97.GetDescription();
                _logger.LogWarning("VNPAY: Invalid signature");
            }
        }
        else
        {
            // No query parameters provided
            response.RspCode = ((int)VnPayIPNResponseCode.CODE_99).ToString("D2");
            response.Message = VnPayIPNResponseCode.CODE_99.GetDescription();
            _logger.LogWarning("VNPAY: No query parameters provided");
        }

        return response;
    }

    public async Task<string> GetPaymentType(IQueryCollection queryParams)
    {
        if (queryParams.Count > 0)
        {
            string vnpHashSecret = _vnPaySetting.HashSecret;
            VnPayLibrary vnPay = new VnPayLibrary();

            // Iterate over query parameters
            foreach (string key in queryParams.Keys)
            {
                // Add only VNPAY-related parameters that start with "vnp_"
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, queryParams[key]);
                }
            }

            return vnPay.GetResponseData(VnPayRequestParam.VNP_ORDER_INFO);
        }
        else
        {
            return string.Empty;
        }
    }
}