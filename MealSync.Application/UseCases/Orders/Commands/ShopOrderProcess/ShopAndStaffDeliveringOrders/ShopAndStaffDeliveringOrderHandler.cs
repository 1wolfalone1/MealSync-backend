using System.Drawing;
using System.Net;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Notifications;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using QRCoder;

namespace MealSync.Application.UseCases.Orders.Commands.ShopOrderProcess.ShopAndStaffDeliveringOrders;

public class ShopAndStaffDeliveringOrderHandler : ICommandHandler<ShopAndStaffDeliveringOrderCommand, Result>
{
    private readonly ICurrentAccountService _currentAccountService;
    private readonly IOrderRepository _orderRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotifierService _notifierService;
    private readonly INotificationFactory _notificationFactory;
    private readonly IShopDeliveryStaffRepository _shopDeliveryStaffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStorageService _storageService;
    private readonly ILogger<ShopAndStaffDeliveringOrderHandler> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly IShopRepository _shopRepository;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public ShopAndStaffDeliveringOrderHandler(ICurrentAccountService currentAccountService, IOrderRepository orderRepository, INotificationRepository notificationRepository, INotificationFactory notificationFactory, IShopDeliveryStaffRepository shopDeliveryStaffRepository, IUnitOfWork unitOfWork, IStorageService storageService, ILogger<ShopAndStaffDeliveringOrderHandler> logger, IAccountRepository accountRepository, IShopRepository shopRepository, INotifierService notifierService, ISystemResourceRepository systemResourceRepository)
    {
        _currentAccountService = currentAccountService;
        _orderRepository = orderRepository;
        _notificationRepository = notificationRepository;
        _notificationFactory = notificationFactory;
        _shopDeliveryStaffRepository = shopDeliveryStaffRepository;
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _logger = logger;
        _accountRepository = accountRepository;
        _shopRepository = shopRepository;
        _notifierService = notifierService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(ShopAndStaffDeliveringOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate
        Validate(request);

        try
        {
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);
            var orders = _orderRepository.GetByIds(request.Ids.ToList());
            foreach (var order in orders)
            {
                var hashString = order.Id + order.DeliveryPackageId + order.CustomerId + order.PhoneNumber + order.FullName;
                var token = BCrypUnitls.Hash(hashString);
                Bitmap qrCode = GenerateOrderQRCodeBitmap(order, token);
                var imageUrl = await _storageService.UploadFileAsync(qrCode).ConfigureAwait(false);
                order.QrScanToDeliveried = imageUrl;
                order.Status = OrderStatus.Delivering;
            }

            _orderRepository.UpdateRange(orders);
            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            // Notifor customer
            var listNoti = new List<Notification>();
            var account = _currentAccountService.GetCurrentAccount();
            long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
            var shop = _shopRepository.GetById(shopId);
            foreach (var order in orders)
            {
                var noti = _notificationFactory.CreateOrderDeliveringNotification(order, shop);
                listNoti.Add(noti);
            }

            _notifierService.NotifyRangeAsync(listNoti);

            return Result.Success(new
            {
                Message = MessageCode.I_ORDER_DELIVERING.GetDescription(),
                Code = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_DELIVERING.GetDescription(),  string.Join(", ", request.Ids)),
            });
        }
        catch (Exception e)
        {
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private void Validate(ShopAndStaffDeliveringOrderCommand request)
    {
        var idsNotFound = new List<long>();
        var account = _currentAccountService.GetCurrentAccount();
        long shopId = account.RoleId == (int)Domain.Enums.Roles.ShopOwner ? account.Id : _shopDeliveryStaffRepository.GetById(account.Id).ShopId;
        foreach (var id in request.Ids)
        {
            if (_orderRepository.Get(o => o.Id == id && o.ShopId == shopId).SingleOrDefault() == default)
                idsNotFound.Add(id);
        }

        if (idsNotFound.Count > 0)
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { string.Join(", ", idsNotFound) }, HttpStatusCode.NotFound);
    }

    public Bitmap GenerateOrderQRCodeBitmap(Order orderInfo, string token)
    {
        string orderDataJson = JsonConvert.SerializeObject(orderInfo);
        using (var qrGenerator = new QRCodeGenerator())
        {
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(orderDataJson, QRCodeGenerator.ECCLevel.Q);
            using (var qrCode = new QRCoder.QRCode(qrCodeData))
            {
                return qrCode.GetGraphic(20); // Creates a Bitmap QR code
            }
        }
    }
}