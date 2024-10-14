// using MealSync.API.Shared;
// using MealSync.Application.Common.Repositories;
// using MealSync.Application.Common.Services.Payments.VnPay;
// using MealSync.Application.Shared;
// using Microsoft.AspNetCore.Mvc;
// using MealSync.Domain.Entities;
// using MealSync.Domain.Enums;
//
// namespace MealSync.API.Controllers;
//
// [Route(Endpoints.BASE)]
// public class OrderController : BaseApiController
// {
//     private readonly IVnPayPaymentService _paymentService;
//     private readonly ILogger<OrderController> _logger;
//     private readonly IActivityLogRepository _activityLogRepository;
//     private readonly IUnitOfWork _unitOfWork;
//
//     public OrderController(IVnPayPaymentService paymentService, ILogger<OrderController> logger, IActivityLogRepository activityLogRepository, IUnitOfWork unitOfWork)
//     {
//         _paymentService = paymentService;
//         _logger = logger;
//         _activityLogRepository = activityLogRepository;
//         _unitOfWork = unitOfWork;
//     }
//
//     [HttpGet(Endpoints.CREATE_ORDER)]
//     public async Task<IActionResult> CreateOrder()
//     {
//         // Get the current time with the correct offset
//         var time = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));
//         string format = "dd-MM-yyyy HH:mm:ss zzz";
//         string dateTimeString = time.ToString(format);
//
//         _logger.LogInformation(dateTimeString);
//
//         var payment = new Payment
//         {
//             Id = 2900,
//             Amount = 30000,
//             OrderId = 3000,
//             CreatedDate = time,
//         };
//
//         // Create the payment URL asynchronously
//         var paymentUrl = await _paymentService.CreatePaymentUrl(payment).ConfigureAwait(false);
//         return HandleResult(Result.Success(paymentUrl));
//     }
//
//     [HttpGet(Endpoints.CREATE_REFUND)]
//     public async Task<IActionResult> CreateRefund()
//     {
//         string format = "dd-MM-yyyy HH:mm:ss zzz";
//         DateTimeOffset dto = DateTimeOffset.ParseExact("10-10-2024 23:22:30 +07:00", format, null);
//         var payment = new Payment
//         {
//             Id = 290,
//             Amount = 30000,
//             OrderId = 300,
//             VnPayTransactionNo = 14609928,
//             CreatedDate = dto,
//         };
//         var response = await _paymentService.CreateRefund(payment).ConfigureAwait(false);
//         return HandleResult(Result.Success(response));
//     }
//
//     [HttpGet(Endpoints.GET_IPN)]
//     public async Task<IActionResult> GetIPN()
//     {
//         var payment = new Payment
//         {
//             Id = 2900,
//             Amount = 30000,
//             OrderId = 3000,
//             Status = PaymentStatus.Pending,
//         };
//
//         var queryParams = HttpContext.Request.Query;
//         var response = await _paymentService.GetIPN(queryParams, payment);
//
//         _logger.LogInformation("----------------------GetIPN----------------------");
//         _logger.LogInformation("RspCode: " + response.RspCode);
//         _logger.LogInformation("Message: " + response.Message);
//         _logger.LogInformation("------------------------END-----------------------");
//         ActivityLog activityLog = new ActivityLog
//         {
//             AccountId = 1,
//             ActionType = ModeratorActionTypes.Create,
//             TargetType = ModeratorTargetTypes.Order,
//             TargetId = 1,
//             ActionDetail = "RspCode: " + response.RspCode + " Message: " + response.Message,
//             IsSuccess = true
//         };
//         try
//         {
//             // Begin transaction
//             await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
//             await _activityLogRepository.AddAsync(activityLog).ConfigureAwait(false);
//             await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);
//             return HandleResult(Result.Success(response));
//         }
//         catch (Exception e)
//         {
//             // Rollback when exception
//             _unitOfWork.RollbackTransaction();
//             _logger.LogError(e, e.Message);
//             throw new("Internal Server Error");
//         }
//     }
// }