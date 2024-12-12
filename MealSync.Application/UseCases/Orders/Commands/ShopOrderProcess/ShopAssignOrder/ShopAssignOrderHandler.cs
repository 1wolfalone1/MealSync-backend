using System.Net;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Chat;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopDeliveringOrder;

public class ShopAssignOrderHandler : ICommandHandler<ShopAssignOrderCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ShopAssignOrderCommand> _logger;
    private readonly IDeliveryPackageRepository _deliveryPackageRepository;
    private readonly INotificationFactory _notificationFactory;
    private readonly INotifierService _notifierService;
    private readonly IShopRepository _shopRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;
    private const string QR_KEY = "QR_KEY";
    private readonly IConfiguration _configuration;
    private readonly IChatService _chatService;

    public ShopAssignOrderHandler(IOrderRepository orderRepository, ICurrentPrincipalService currentPrincipalService, IUnitOfWork unitOfWork, ILogger<ShopAssignOrderCommand> logger,
        IDeliveryPackageRepository deliveryPackageRepository, INotificationFactory notificationFactory, INotifierService notifierService, IShopRepository shopRepository, IAccountRepository accountRepository,
        ISystemResourceRepository systemResourceRepository, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IMapper mapper, IStorageService storageService, IConfiguration configuration, IChatService chatService)
    {
        _orderRepository = orderRepository;
        _currentPrincipalService = currentPrincipalService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _deliveryPackageRepository = deliveryPackageRepository;
        _notificationFactory = notificationFactory;
        _notifierService = notifierService;
        _shopRepository = shopRepository;
        _accountRepository = accountRepository;
        _systemResourceRepository = systemResourceRepository;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _mapper = mapper;
        _storageService = storageService;
        _configuration = configuration;
        _chatService = chatService;
    }

    public async Task<Result<Result>> Handle(ShopAssignOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        // Warning
        var order = _orderRepository.GetById(request.OrderId);
        if (!request.IsConfirm.Value)
        {
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            var intendedReceiveDateTime = new DateTime(
                order.IntendedReceiveDate.Year,
                order.IntendedReceiveDate.Month,
                order.IntendedReceiveDate.Day,
                order.StartTime / 100,
                order.StartTime % 100,
                0);
            var endTime = new DateTimeOffset(intendedReceiveDateTime, TimeSpan.FromHours(7)).AddHours(-OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS);
            if (now < endTime && order.DeliveryPackageId == null)
            {
                var diffDate = endTime.AddHours(OrderConstant.TIME_WARNING_SHOP_ASSIGN_ORDER_EARLY_IN_HOURS) - now;
                return Result.Warning(new
                {
                    Code = MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_ASSIGN_EARLY.GetDescription(),
                        new string[] { order.Id.ToString(), TimeFrameUtils.GetTimeFrameString(order.StartTime, order.EndTime), $"{diffDate.Hours}h{diffDate.Minutes}p" }),
                });
            }

            if (order.DeliveryPackageId != null && order.Status == OrderStatus.Preparing)
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_ORDER_IN_OTHER_DELIVERY_PACKAGE.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_IN_OTHER_DELIVERY_PACKAGE.GetDescription(), order.Id),
                });
            }

            // Warning order in status delivering do you sure change to other staff
            var deliveryPackage = _deliveryPackageRepository.GetById(order.DeliveryPackageId);
            if (order.Status == OrderStatus.Delivering && (request.ShopDeliveryStaffId.HasValue && deliveryPackage.ShopDeliveryStaffId != request.ShopDeliveryStaffId
                                                           || !request.ShopDeliveryStaffId.HasValue && _currentPrincipalService.CurrentPrincipalId == deliveryPackage.ShopId))
            {
                return Result.Warning(new
                {
                    Code = MessageCode.W_ORDER_DELIVERING_RE_ASSIGN.GetDescription(),
                    Message = _systemResourceRepository.GetByResourceCode(MessageCode.W_ORDER_DELIVERING_RE_ASSIGN.GetDescription(),
                        new string[] { order.Id.ToString() }),
                });
            }
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            DeliveryPackage dp;
            if (order.DeliveryPackageId == null)
            {
                // Get delivery package of shipper in this frame if not exist create
                dp = await CreateOrAddOrderInDeliveryPackageAsync(order, request).ConfigureAwait(false);
            }
            else
            {
                dp = await ReAssignOrderInDeliveryPackageAsync(order, request).ConfigureAwait(false);

                // If order in delivering status must be re generate qr code
                if (order.Status == OrderStatus.Delivering)
                {
                    var hashString = order.Id + dp.Id + order.CustomerId + order.PhoneNumber + order.FullName + _configuration[QR_KEY];
                    var token = BCrypUnitls.Hash(hashString);
                    var shipperId = dp.ShopId.HasValue ? dp.ShopId.Value : dp.ShopDeliveryStaffId.Value;
                    var qrCode = await GenerateOrderQRCodeBitmapAsync(order, token, shipperId).ConfigureAwait(false);
                    var imageUrl = await _storageService.UploadFileAsync(qrCode).ConfigureAwait(false);
                    order.QrScanToDeliveried = imageUrl;
                }
            }

            // Save an history assign
            var shipperIdAssign = dp.ShopId.HasValue ? dp.ShopId.Value : dp.ShopDeliveryStaffId.Value;
            if (order.HistoryAssignJson != null)
            {
                var history = JsonConvert.DeserializeObject<List<HistoryAssign>>(order.HistoryAssignJson);
                history.Add(new HistoryAssign()
                {
                    Id = shipperIdAssign,
                    AssignDate = DateTimeOffset.Now,
                });
                order.HistoryAssignJson = JsonConvert.SerializeObject(history);

                // Send noti to add shipper
                if (shipperIdAssign != order.ShopId && history.All(h => h.Id != shipperIdAssign))
                {
                    var shipperAccount = _accountRepository.GetById(shipperIdAssign);
                    var notificationJoinRoom = _notificationFactory.CreateJoinRoomToCustomerNotification(order, shipperAccount);

                    _chatService.OpenOrCloseRoom(new AddChat()
                    {
                        IsOpen = true,
                        RoomId = order.Id,
                        UserId = shipperIdAssign,
                        Notification = notificationJoinRoom,
                    });
                }
            }
            else
            {
                var history = new List<HistoryAssign>();
                history.Add(new HistoryAssign()
                {
                    Id = shipperIdAssign,
                    AssignDate = DateTimeOffset.Now,
                });
                order.HistoryAssignJson = JsonConvert.SerializeObject(history);

                // Send noti to add shipper
                if (shipperIdAssign != order.ShopId)
                {
                    var shipperAccount = _accountRepository.GetById(shipperIdAssign);
                    var notificationJoinRoom = _notificationFactory.CreateJoinRoomToCustomerNotification(order, shipperAccount);

                    _chatService.OpenOrCloseRoom(new AddChat()
                    {
                        IsOpen = true,
                        RoomId = order.Id,
                        UserId = shipperIdAssign,
                        Notification = notificationJoinRoom,
                    });
                }
            }

            _orderRepository.Update(order);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            var listNoti = new List<Notification>();
            var shop = _shopRepository.GetById(order.ShopId);

            // Noti to shop staff about delivery package
            if (request.ShopDeliveryStaffId != null)
            {
                var accShip = _accountRepository.GetById(request.ShopDeliveryStaffId.Value);
                var notiShopStaff = _notificationFactory.CreateOrderAssignedToStaffNotification(dp, accShip, shop);
                listNoti.Add(notiShopStaff);
            }

            _notifierService.NotifyRangeAsync(listNoti);

            var deliveryPackageResponse = _deliveryPackageRepository.Get(deliveryPackage => deliveryPackage.Id == dp.Id)
                .Include(dp => dp.ShopDeliveryStaff)
                .ThenInclude(sds => sds.Account)
                .Include(dp => dp.Shop)
                .ThenInclude(s => s.Account).Single();
            return Result.Success(_mapper.Map<OrderDetailForShopResponse.ShopDeliveryStaffInShopOrderDetail>(deliveryPackageResponse));
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task<DeliveryPackage> CreateOrAddOrderInDeliveryPackageAsync(Order order, ShopAssignOrderCommand request)
    {
        var shipId = request.ShopDeliveryStaffId ?? _currentPrincipalService.CurrentPrincipalId.Value;
        var deliveryPackage = _deliveryPackageRepository.GetPackageByShipIdAndTimeFrame(request.ShopDeliveryStaffId == null, shipId, order.StartTime, order.EndTime);
        if (deliveryPackage != null)
        {
            // Add order to current package
            deliveryPackage.Orders.Add(order);
            _deliveryPackageRepository.Update(deliveryPackage);
            return deliveryPackage;
        }
        else
        {
            // Need create new delivery package
            var dp = new DeliveryPackage()
            {
                ShopDeliveryStaffId = request.ShopDeliveryStaffId,
                ShopId = request.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
                DeliveryDate = order.IntendedReceiveDate,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                Status = DeliveryPackageStatus.InProcess,

            };
            await _deliveryPackageRepository.AddAsync(dp).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);
            order.DeliveryPackageId = dp.Id;
            if (request.ShopDeliveryStaffId != null)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShopDeliveryStaffId.Value);
                // shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
            }

            return dp;
        }
    }

    private async Task<DeliveryPackage> ReAssignOrderInDeliveryPackageAsync(Order order, ShopAssignOrderCommand request)
    {
        var shipId = request.ShopDeliveryStaffId ?? _currentPrincipalService.CurrentPrincipalId.Value;
        var deliveryPackage = _deliveryPackageRepository.GetPackageByShipIdAndTimeFrame(request.ShopDeliveryStaffId == null, shipId, order.StartTime, order.EndTime);
        if (deliveryPackage != null)
        {
            // Add order to current package
            var oldDeliveryPackageId = order.DeliveryPackageId;
            order.DeliveryPackageId = deliveryPackage.Id;
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            // Delete delivery package if have no order left
            var deliveryPackageDelete = _deliveryPackageRepository.Get(dp => dp.Id == oldDeliveryPackageId)
                .Include(dp => dp.Orders).Single();

            if (deliveryPackageDelete.Orders == null || deliveryPackageDelete.Orders.Count() == 0)
            {
                _deliveryPackageRepository.Remove(deliveryPackageDelete);
            }

            return deliveryPackage;
        }
        else
        {
            // Need create new delivery package
            var dp = new DeliveryPackage()
            {
                ShopDeliveryStaffId = request.ShopDeliveryStaffId,
                ShopId = request.ShopDeliveryStaffId == null ? _currentPrincipalService.CurrentPrincipalId.Value : null,
                DeliveryDate = order.IntendedReceiveDate,
                StartTime = order.StartTime,
                EndTime = order.EndTime,
                Status = DeliveryPackageStatus.InProcess,

            };
            await _deliveryPackageRepository.AddAsync(dp).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            // Update to order
            var oldDeliveryPackageId = order.DeliveryPackageId;
            order.DeliveryPackageId = dp.Id;
            _orderRepository.Update(order);
            await _unitOfWork.SaveChangesAsync().ConfigureAwait(false);

            // Delete delivery package if have no order left
            var deliveryPackageDelete = _deliveryPackageRepository.Get(dp => dp.Id == oldDeliveryPackageId)
                .Include(dp => dp.Orders).Single();

            if (deliveryPackageDelete.Orders == null || deliveryPackageDelete.Orders.Count() == 0)
            {
                _deliveryPackageRepository.Remove(deliveryPackageDelete);
            }

            if (request.ShopDeliveryStaffId != null)
            {
                var shopDeliveryStaff = _shopDeliveryStaffRepository.GetById(request.ShopDeliveryStaffId.Value);
                // shopDeliveryStaff.Status = ShopDeliveryStaffStatus.Busy;
                _shopDeliveryStaffRepository.Update(shopDeliveryStaff);
            }

            return dp;
        }
    }

    private void Validate(ShopAssignOrderCommand request)
    {
        var order = _orderRepository
            .Get(o => o.Id == request.OrderId && o.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
            .Include(o => o.DeliveryPackage).SingleOrDefault();
        if (order == default)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId }, HttpStatusCode.NotFound);

        if (order.Status != OrderStatus.Preparing && order.Status != OrderStatus.Delivering)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_IN_CORRECT_STATUS.GetDescription(), new object[] { request.OrderId });

        if (order.IntendedReceiveDate.Date != TimeFrameUtils.GetCurrentDateInUTC7().Date)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_DELIVERING_IN_WRONG_DATE.GetDescription(), new object[] { order.Id, order.IntendedReceiveDate.Date.ToString("dd-MM-yyyy") });

        var currentDateTime = TimeFrameUtils.GetCurrentDateInUTC7();
        var startEndTime = TimeFrameUtils.GetStartTimeEndTimeToDateTime(order.IntendedReceiveDate, order.StartTime, order.EndTime);
        if (currentDateTime.DateTime > startEndTime.EndTime)
            throw new InvalidBusinessException(MessageCode.E_ORDER_OVER_TIME.GetDescription(), new object[] { request.OrderId });

        if (request.ShopDeliveryStaffId != null)
        {
            var shopDeliveryStaff = _shopDeliveryStaffRepository.Get(sds => sds.Id == request.ShopDeliveryStaffId && sds.ShopId == _currentPrincipalService.CurrentPrincipalId.Value)
                .Include(sds => sds.Account).SingleOrDefault();
            if (shopDeliveryStaff == null || shopDeliveryStaff.ShopId != _currentPrincipalService.CurrentPrincipalId)
                throw new InvalidBusinessException(MessageCode.E_ORDER_ASSIGN_NOT_FOUND_SHOP_STAFF.GetDescription(), new object[] { request.ShopDeliveryStaffId.Value });

            if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.Offline)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_OFFLINE_STATUS.GetDescription(), new object[] { request.ShopDeliveryStaffId });

            if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status == AccountStatus.Deleted)
                throw new InvalidBusinessException(MessageCode.E_SHOP_DELIVERY_STAFF_NOT_FOUND.GetDescription(), new object[] { request.ShopDeliveryStaffId });

            if (shopDeliveryStaff.Status == ShopDeliveryStaffStatus.InActive && shopDeliveryStaff.Account.Status != AccountStatus.Deleted)
                throw new InvalidBusinessException(MessageCode.E_DELIVERY_PACKAGE_STAFF_IN_ACTIVE.GetDescription(), new object[] { request.ShopDeliveryStaffId });
        }
    }

    public async Task<Image<Rgba32>> GenerateOrderQRCodeBitmapAsync(Order orderInfo, string token, long shipperId)
    {
        string orderDataJson = JsonConvert.SerializeObject(new
        {
            OrderId = orderInfo.Id,
            CustomerId = orderInfo.CustomerId,
            ShipperId = shipperId,
            OrderDate = orderInfo.OrderDate,
            Token = token,
        });

        return await _storageService.GenerateQRCodeWithLogoAsync(orderDataJson).ConfigureAwait(false);
    }
}