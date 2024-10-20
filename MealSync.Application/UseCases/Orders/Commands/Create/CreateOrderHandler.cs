using System.Text.Json;
using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Services.Payments.VnPay;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using Microsoft.Extensions.Logging;

namespace MealSync.Application.UseCases.Orders.Commands.Create;

public class CreateOrderHandler : ICommandHandler<CreateOrderCommand, Result>
{
    private readonly ILogger<CreateOrderCommand> _logger;
    private readonly IShopRepository _shopRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IFoodOperatingSlotRepository _foodOperatingSlotRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;
    private readonly IOptionRepository _optionRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly ICommissionConfigRepository _commissionConfigRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IVnPayPaymentService _paymentService;
    private readonly ISystemResourceRepository _systemResourceRepository;

    public CreateOrderHandler(ILogger<CreateOrderCommand> logger, IShopRepository shopRepository, IShopDormitoryRepository shopDormitoryRepository, IBuildingRepository buildingRepository, IFoodRepository foodRepository,
        IFoodOperatingSlotRepository foodOperatingSlotRepository, IOperatingSlotRepository operatingSlotRepository, IFoodOptionGroupRepository foodOptionGroupRepository, IOptionRepository optionRepository,
        IPromotionRepository promotionRepository, ICurrentPrincipalService currentPrincipalService, ICommissionConfigRepository commissionConfigRepository, IOrderRepository orderRepository, IUnitOfWork unitOfWork,
        IMapper mapper, IVnPayPaymentService paymentService, ISystemResourceRepository systemResourceRepository)
    {
        _logger = logger;
        _shopRepository = shopRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _buildingRepository = buildingRepository;
        _foodRepository = foodRepository;
        _foodOperatingSlotRepository = foodOperatingSlotRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _foodOptionGroupRepository = foodOptionGroupRepository;
        _optionRepository = optionRepository;
        _promotionRepository = promotionRepository;
        _currentPrincipalService = currentPrincipalService;
        _commissionConfigRepository = commissionConfigRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _paymentService = paymentService;
        _systemResourceRepository = systemResourceRepository;
    }

    public async Task<Result<Result>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!;
        var shop = await _shopRepository.GetByIdIncludeLocation(request.ShopId).ConfigureAwait(false);
        var buildingOrder = await _buildingRepository.GetByIdIncludeLocation(request.BuildingId).ConfigureAwait(false);
        var now = DateTimeOffset.Now.ToOffset(TimeSpan.FromHours(7));

        // Validate business rules for the shop.
        await ValidateShopRequest(request, shop, buildingOrder).ConfigureAwait(false);

        // Validate order time
        ValidateOrderTime(request, now);

        var shopOperatingSlot = await _operatingSlotRepository.GetAvailableForTimeRangeOrder(
                request.ShopId, request.OrderTime.StartTime, request.OrderTime.EndTime)
            .ConfigureAwait(false);

        // Validate business rules for the operating slot of shop.
        ValidateOrderTimeRequest(request, shopOperatingSlot);

        // Validate business rules for the food.
        var validateFoodResult = await ValidateFoodRequest(request, shopOperatingSlot).ConfigureAwait(false);

        // Validate business rules for the voucher.
        var promotion = await ValidatePromotionRequest(request, validateFoodResult.TotalFoodPrice, now).ConfigureAwait(false);

        var commissionConfig = _commissionConfigRepository.GetCommissionConfig();

        // Create payment
        Payment payment = new Payment
        {
            Amount = request.TotalOrder,
            Status = PaymentStatus.Pending,
            Type = PaymentTypes.Payment,
            PaymentMethods = request.PaymentMethod,
            PaymentThirdPartyId = null,
            PaymentThirdPartyContent = null,
        };

        var payments = new List<Payment>();
        payments.Add(payment);

        // Create new shop location
        var shopLocation = new Location
        {
            Address = shop!.Location.Address,
            Latitude = shop.Location.Latitude,
            Longitude = shop.Location.Longitude,
        };

        // Create new customer building
        var customerLocation = new Location
        {
            Address = buildingOrder!.Location.Address,
            Latitude = buildingOrder.Location.Latitude,
            Longitude = buildingOrder.Location.Longitude,
        };

        // Create new order
        Order order = new Order
        {
            PromotionId = request.VoucherId.HasValue && request.VoucherId != 0 ? request.VoucherId.Value : null,
            ShopId = request.ShopId,
            CustomerId = customerId.Value,
            DeliveryPackageId = null,
            ShopLocation = shopLocation,
            CustomerLocation = customerLocation,
            BuildingId = request.BuildingId,
            BuildingName = buildingOrder!.Name,
            Status = OrderStatus.Pending,
            Note = request.Note,
            ShippingFee = 0,
            TotalPrice = request.TotalFoodCost,
            TotalPromotion = request.TotalDiscount,
            ChargeFee = commissionConfig / 100 * request.TotalOrder,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            OrderDate = now,
            IntendedReceiveDate = request.OrderTime.IsOrderNextDay ? now.AddDays(1).Date : now.Date,
            ReceiveAt = null,
            CompletedAt = null,
            StartTime = request.OrderTime.StartTime,
            EndTime = request.OrderTime.EndTime,
            QrScanToDeliveried = null,
            DeliverySuccessImageUrl = null,
            IsRefund = false,
            IsReport = false,
            Reason = null,
            OrderDetails = validateFoodResult.OrderDetails,
            Payments = payments,
        };

        if (shop.IsAutoOrderConfirmation && !request.OrderTime.IsOrderNextDay)
        {
            var startFrameMinutes = TimeUtils.ConvertToMinutes(request.OrderTime.StartTime); // Total minutes for start time
            var minOrderMinutes = shop.MinOrderHoursInAdvance * 60; // Min constraint in minutes
            var maxOrderMinutes = shop.MaxOrderHoursInAdvance * 60; // Max constraint in minutes
            var currentTimeMinutes = (now.Hour * 60) + now.Minute; // Convert current time to total minutes
            var minAllowedTime = startFrameMinutes - maxOrderMinutes < 0 ? 0 : startFrameMinutes - maxOrderMinutes;
            var maxAllowedTime = startFrameMinutes - minOrderMinutes;

            if (maxAllowedTime >= 0 && currentTimeMinutes >= minAllowedTime && currentTimeMinutes <= maxAllowedTime)
            {
                order.Status = OrderStatus.Confirmed;
            }
            else
            {
                order.Status = OrderStatus.Pending;
            }
        }

        try
        {
            // Begin transaction
            await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

            await _orderRepository.AddAsync(order).ConfigureAwait(false);

            // Update promotion usage if present
            if (promotion != default)
            {
                promotion.NumberOfUsed += 1;
                _promotionRepository.Update(promotion);
            }

            // Update total order of shop
            shop.TotalOrder += 1;
            _shopRepository.Update(shop);

            // Update total order of food
            foreach (var food in validateFoodResult.foods)
            {
                food.TotalOrder += 1;
                _foodRepository.Update(food);
            }

            await _unitOfWork.CommitTransactionAsync().ConfigureAwait(false);

            CreateOrderResponse response = new CreateOrderResponse()
            {
                PaymentMethod = request.PaymentMethod,
                PaymentLink = request.PaymentMethod == PaymentMethods.VnPay ? await _paymentService.CreatePaymentUrl(payment).ConfigureAwait(false) : null,
                Order = _mapper.Map<OrderResponse>(order),
                Message = _systemResourceRepository.GetByResourceCode(MessageCode.I_ORDER_SUCCESS.GetDescription()) ?? string.Empty,
            };

            // Todo: Notification for shop

            return Result.Create(response);
        }
        catch (Exception e)
        {
            // Rollback when exception
            _unitOfWork.RollbackTransaction();
            _logger.LogError(e, e.Message);
            throw new("Internal Server Error");
        }
    }

    private static void ValidateOrderTime(CreateOrderCommand request, DateTimeOffset now)
    {
        var startTimeInMinutes = TimeUtils.ConvertToMinutes(request.OrderTime.StartTime);
        var currentTimeMinutes = (now.Hour * 60) + now.Minute;
        if (!request.OrderTime.IsOrderNextDay && startTimeInMinutes < currentTimeMinutes)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_DELIVERY_START_TIME_EXCEEDED.GetDescription());
        }
        else
        {
            // Do nothing
        }
    }

    private async Task<Promotion?> ValidatePromotionRequest(CreateOrderCommand request, double totalFoodPrice, DateTimeOffset now)
    {
        // Check if a voucher is provided in the request
        if (request.VoucherId != default && request.VoucherId != 0)
        {
            // Retrieve the promotion by ID and ShopId
            var promotion = await _promotionRepository.GetByIdAndShopId(request.VoucherId.Value, request.ShopId).ConfigureAwait(false);

            // Check if the promotion exists
            if (promotion == default)
            {
                throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_FOUND.GetDescription(), new object[] { request.VoucherId });
            }
            else
            {
                if (promotion.Status == PromotionStatus.UnActive)
                {
                    // Throw an exception if the promotion is inactive
                    throw new InvalidBusinessException(MessageCode.E_PROMOTION_INACTIVE.GetDescription(), new object[] { promotion.Title });
                }
                else if (promotion.Status == PromotionStatus.Delete)
                {
                    // Throw an exception if the promotion is deleted
                    throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_FOUND.GetDescription(), new object[] { request.VoucherId });
                }
                else
                {
                    if (promotion.NumberOfUsed == promotion.UsageLimit)
                    {
                        // Throw an exception if the promotion usage limit has been reached
                        throw new InvalidBusinessException(MessageCode.E_PROMOTION_OUT_OF_AVAILABLE.GetDescription(), new object[] { promotion.Title });
                    }
                    else if (now < promotion.StartDate)
                    {
                        // Throw an exception if the promotion hasn't started yet
                        throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_APPLY_FOR_THIS_TIME.GetDescription());
                    }
                    else if (now > promotion.EndDate)
                    {
                        // Throw an exception if the promotion has expired
                        throw new InvalidBusinessException(MessageCode.E_PROMOTION_EXPIRED.GetDescription());
                    }
                    else if (promotion.Type == PromotionTypes.ShopPromotion && promotion.ApplyType == PromotionApplyTypes.Percent)
                    {
                        // Ensure the order meets the minimum value condition
                        if (promotion.MinOrdervalue > totalFoodPrice)
                        {
                            throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_ENOUGH_CONDITION.GetDescription());
                        }
                        else if (promotion.AmountRate / 100 * totalFoodPrice > promotion.MaximumApplyValue && request.TotalDiscount - promotion.MaximumApplyValue != 0.0)
                        {
                            // Throw an exception exceeds the maximum allowed value
                            throw new InvalidBusinessException(MessageCode.E_ORDER_INCORRECT_DISCOUNT_AMOUNT.GetDescription());
                        }
                        else if (promotion.AmountRate / 100 * totalFoodPrice <= promotion.MaximumApplyValue && request.TotalDiscount - (promotion.AmountRate / 100 * totalFoodPrice) != 0.0)
                        {
                            throw new InvalidBusinessException(MessageCode.E_ORDER_INCORRECT_DISCOUNT_AMOUNT.GetDescription());
                        }
                        else
                        {
                            // Do nothing
                            return promotion;
                        }
                    }
                    else if (promotion.Type == PromotionTypes.ShopPromotion && promotion.ApplyType == PromotionApplyTypes.Absolute)
                    {
                        if (promotion.MinOrdervalue > totalFoodPrice)
                        {
                            throw new InvalidBusinessException(MessageCode.E_PROMOTION_NOT_ENOUGH_CONDITION.GetDescription());
                        }
                        else if (request.TotalDiscount - promotion.AmountValue != 0.0)
                        {
                            throw new InvalidBusinessException(MessageCode.E_ORDER_INCORRECT_DISCOUNT_AMOUNT.GetDescription());
                        }
                        else
                        {
                            // Do nothing
                            return promotion;
                        }
                    }
                    else
                    {
                        // Do nothing
                        return promotion;
                    }
                }
            }
        }
        else
        {
            // Do nothing
        }
        return default;
    }

    private async Task<(double TotalFoodPrice, List<OrderDetail> OrderDetails, List<Food> foods)> ValidateFoodRequest(
        CreateOrderCommand request, OperatingSlot? shopOperatingSlot)
    {
        var orderDetails = new List<OrderDetail>();
        var foods = new List<Food>();
        double totalFoodPrice = 0;

        foreach (var foodRequest in request.Foods)
        {
            double totalPriceOrderDetail = 0;
            var orderDetailDescriptionList = new List<OrderDetailDescriptionDto>();

            // Extract food ID from the request
            var id = long.Parse(foodRequest.Id.Split('-')[0]);
            var food = await _foodRepository.GetByIdAndShopId(id, request.ShopId).ConfigureAwait(false);

            // Check if the food item exists
            if (food == default)
            {
                // Throw an exception if the food is not found
                throw new InvalidBusinessException(MessageCode.E_FOOD_NOT_FOUND.GetDescription(), new object[] { id });
            }
            else
            {
                // Check the status of the food item
                if (food.Status == FoodStatus.UnActive)
                {
                    // Throw an exception if the food is inactive
                    throw new InvalidBusinessException(MessageCode.E_FOOD_INACTIVE.GetDescription(), new object[] { food.Name });
                }
                else if (food.Status == FoodStatus.Delete)
                {
                    // Throw an exception if the food is marked for deletion
                    throw new InvalidBusinessException(MessageCode.E_FOOD_NOT_FOUND.GetDescription(), new object[] { id });
                }
                else
                {
                    // Check if the food item is sold out
                    if (food.IsSoldOut)
                    {
                        throw new InvalidBusinessException(MessageCode.E_FOOD_IS_SOLD_OUT.GetDescription(), new object[] { food.Name });
                    }
                    else if (!await _foodOperatingSlotRepository.ExistedByFoodIdAndOperatingSlotId(food.Id, shopOperatingSlot!.Id).ConfigureAwait(false))
                    {
                        // Throw an exception if the food not sold during the requested order time
                        throw new InvalidBusinessException(
                            MessageCode.E_ORDER_FOOD_NOT_SELL_IN_ORDER_TIME.GetDescription(),
                            new object[] { food.Name, request.OrderTime.StartTime, request.OrderTime.EndTime }
                        );
                    }
                    else
                    {
                        // Add the food price to the total price
                        totalFoodPrice += food.Price * foodRequest.Quantity;
                        totalPriceOrderDetail += food.Price * foodRequest.Quantity;
                        foods.Add(food);
                    }

                    var optionGroupSelectedIds = new List<long>();
                    var orderDetailOptions = new List<OrderDetailOption>();

                    // Validate radio option groups if existed in the food request
                    if (foodRequest.OptionGroupRadio != default && foodRequest.OptionGroupRadio.Count > 0)
                    {
                        foreach (var optionGroupRadioRequest in foodRequest.OptionGroupRadio)
                        {
                            var foodOptionGroup = await _foodOptionGroupRepository.GetActiveOptionGroupByFoodIdAndOptionGroupId(food.Id, optionGroupRadioRequest.Id).ConfigureAwait(false);

                            // Check if the option group is active
                            if (foodOptionGroup == default)
                            {
                                throw new InvalidBusinessException(
                                    MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[] { optionGroupRadioRequest.Id });
                            }
                            else
                            {
                                var option = await _optionRepository.GetActiveByIdAndOptionGroupId(
                                        optionGroupRadioRequest.OptionId, optionGroupRadioRequest.Id)
                                    .ConfigureAwait(false);

                                // Check if the option exists
                                if (option == default)
                                {
                                    throw new InvalidBusinessException(
                                        MessageCode.E_OPTION_OF_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[] { optionGroupRadioRequest.OptionId, optionGroupRadioRequest.Id });
                                }
                                else if (option.IsCalculatePrice)
                                {
                                    // Option affects the price, add it to the total price
                                    totalFoodPrice += option.Price * foodRequest.Quantity;
                                    totalPriceOrderDetail += option.Price * foodRequest.Quantity;
                                }
                                else
                                {
                                    // Do nothing
                                }

                                optionGroupSelectedIds.Add(optionGroupRadioRequest.Id);

                                var optionList = new List<OrderDetailDescriptionDto.OptionDto>();

                                optionList.Add(new OrderDetailDescriptionDto.OptionDto
                                {
                                    OptionTitle = option.Title,
                                    Price = option.Price,
                                    OptionImageUrl = option.ImageUrl,
                                    IsCalculatePrice = option.IsCalculatePrice,
                                });
                                orderDetailDescriptionList.Add(new OrderDetailDescriptionDto
                                {
                                    OptionGroupTitle = foodOptionGroup.OptionGroup.Title,
                                    Options = optionList,
                                });

                                orderDetailOptions.Add(new OrderDetailOption
                                {
                                    OptionId = option.Id,
                                    OptionGroupTitle = foodOptionGroup.OptionGroup.Title!,
                                    OptionTitle = option.Title,
                                    OptionImageUrl = option.ImageUrl,
                                    Price = option.Price,
                                });
                            }
                        }
                    }
                    else
                    {
                        // Do nothing
                    }

                    // Validate checkbox option groups if existed in the food request
                    if (foodRequest.OptionGroupCheckbox != default && foodRequest.OptionGroupCheckbox.Count > 0)
                    {
                        foreach (var optionGroupCheckboxRequest in foodRequest.OptionGroupCheckbox)
                        {
                            var foodOptionGroup = await _foodOptionGroupRepository.GetActiveOptionGroupByFoodIdAndOptionGroupId(food.Id, optionGroupCheckboxRequest.Id).ConfigureAwait(false);

                            // Check if the option group is active
                            if (foodOptionGroup == default)
                            {
                                throw new InvalidBusinessException(
                                    MessageCode.E_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[] { optionGroupCheckboxRequest.Id });
                            }
                            else
                            {
                                var optionList = new List<OrderDetailDescriptionDto.OptionDto>();

                                if (foodOptionGroup.OptionGroup.IsRequire && (
                                        foodOptionGroup.OptionGroup.MinChoices > optionGroupCheckboxRequest.OptionIds.Count
                                        || foodOptionGroup.OptionGroup.MaxChoices < optionGroupCheckboxRequest.OptionIds.Count))
                                {
                                    throw new InvalidBusinessException(
                                        MessageCode.E_ORDER_OPTION_SELECTED_OVER_RANGE_MIN_MAX.GetDescription(),
                                        new object[] { foodOptionGroup.OptionGroup.Title!, foodOptionGroup.OptionGroup.MinChoices, foodOptionGroup.OptionGroup.MaxChoices }
                                    );
                                }

                                // Validate each selected option in the checkbox group
                                foreach (var optionId in optionGroupCheckboxRequest.OptionIds)
                                {
                                    var option = await _optionRepository.GetActiveByIdAndOptionGroupId(
                                        optionId, optionGroupCheckboxRequest.Id).ConfigureAwait(false);

                                    // Check if the option exists
                                    if (option == default)
                                    {
                                        throw new InvalidBusinessException(
                                            MessageCode.E_OPTION_OF_OPTION_GROUP_NOT_FOUND.GetDescription(), new object[] { optionId, optionGroupCheckboxRequest.Id });
                                    }
                                    else if (option.IsCalculatePrice)
                                    {
                                        // Option affects the price, add it to the total price
                                        totalFoodPrice += option.Price * foodRequest.Quantity;
                                        totalPriceOrderDetail += option.Price * foodRequest.Quantity;
                                    }
                                    else
                                    {
                                        // Do nothing
                                    }

                                    optionList.Add(new OrderDetailDescriptionDto.OptionDto
                                    {
                                        OptionTitle = option.Title,
                                        Price = option.Price,
                                        OptionImageUrl = option.ImageUrl,
                                        IsCalculatePrice = option.IsCalculatePrice,
                                    });

                                    orderDetailOptions.Add(new OrderDetailOption
                                    {
                                        OptionId = option.Id,
                                        OptionGroupTitle = foodOptionGroup.OptionGroup.Title!,
                                        OptionTitle = option.Title,
                                        OptionImageUrl = option.ImageUrl,
                                        Price = option.Price,
                                    });
                                }

                                optionGroupSelectedIds.Add(optionGroupCheckboxRequest.Id);
                                orderDetailDescriptionList.Add(new OrderDetailDescriptionDto
                                {
                                    OptionGroupTitle = foodOptionGroup.OptionGroup.Title,
                                    Options = optionList,
                                });
                            }
                        }
                    }
                    else
                    {
                        // Do nothing
                    }

                    // Validate all required option is choose
                    var idsOptionGroupRequired = await _foodOptionGroupRepository.GetAllIdsRequiredByFoodId(food.Id).ConfigureAwait(false);
                    bool allIdsContained = idsOptionGroupRequired.All(ogId => optionGroupSelectedIds.Contains(ogId));

                    if (!allIdsContained)
                    {
                        throw new InvalidBusinessException(MessageCode.E_ORDER_OPTION_GROUP_REQUIRED_NOT_SELECT.GetDescription());
                    }

                    // Add to order details
                    var options = new JsonSerializerOptions
                        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
                    var description = JsonSerializer.Serialize(orderDetailDescriptionList, options);

                    orderDetails.Add(new OrderDetail
                    {
                        FoodId = food.Id,
                        Quantity = foodRequest.Quantity,
                        BasicPrice = food.Price,
                        TotalPrice = totalPriceOrderDetail,
                        Description = description,
                        OrderDetailOptions = orderDetailOptions,
                    });
                }
            }
        }

        if (totalFoodPrice - request.TotalFoodCost != 0.0)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_INCORRECT_TOTAL_FOOD_COST.GetDescription());
        }
        else
        {
            return (totalFoodPrice, orderDetails, foods);
        }
    }

    private static void ValidateOrderTimeRequest(CreateOrderCommand request, OperatingSlot? shopOperatingSlot)
    {
        if (shopOperatingSlot == default)
        {
            throw new InvalidBusinessException(
                MessageCode.E_ORDER_SHOP_NOT_SELL_IN_ORDER_TIME.GetDescription(),
                new object[] { request.OrderTime.StartTime, request.OrderTime.EndTime }
            );
        }
    }

    private async Task ValidateShopRequest(CreateOrderCommand request, Shop? shop, Building? buildingOrder)
    {

        // Check existing shop
        if (shop == default)
        {
            // If the shop doesn't exist, throw an exception indicating the shop was not found
            throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
        }
        else
        {
            // Check the status of the shop
            if (shop.Status == ShopStatus.InActive)
            {
                // If the shop is inactive, throw an exception
                throw new InvalidBusinessException(MessageCode.E_SHOP_INACTIVE.GetDescription(), new object[] { shop.Name });
            }
            else if (shop.Status == ShopStatus.Banning || shop.Status == ShopStatus.Banned)
            {
                // If the shop is banned or in the process of being banned, throw an exception
                throw new InvalidBusinessException(MessageCode.E_SHOP_BANNED.GetDescription(), new object[] { shop.Name });
            }
            else if (shop.Status == ShopStatus.Deleted || shop.Status == ShopStatus.UnApprove)
            {
                // If the shop is deleted or not approved, treat it as not found
                throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_FOUND.GetDescription(), new object[] { request.ShopId });
            }
            else
            {
                // Retrieve the building by its ID
                if (buildingOrder == default)
                {
                    // If the building doesn't exist, throw an exception
                    throw new InvalidBusinessException(MessageCode.E_BUILDING_NOT_FOUND.GetDescription(), new object[] { request.BuildingId });
                }
                else if (!await _shopDormitoryRepository.CheckExistedByShopIdAndDormitoryId(request.ShopId, buildingOrder.DormitoryId).ConfigureAwait(false))
                {
                    // If the shop is not associated with the building's dormitory, throw an exception
                    throw new InvalidBusinessException(MessageCode.E_SHOP_DORMITORY_NOT_FOUND.GetDescription(), new object[] { buildingOrder.Name });
                }
                else if (shop.IsReceivingOrderPaused)
                {
                    // If the shop has paused order reception, throw an exception
                    throw new InvalidBusinessException(MessageCode.E_SHOP_RECEIVING_ORDER_PAUSED.GetDescription(), new object[] { shop.Name });
                }
                else if (request.OrderTime.IsOrderNextDay && !shop.IsAcceptingOrderNextDay)
                {
                    // If the order is scheduled for the next day but the shop doesn't accept such orders, throw an exception
                    throw new InvalidBusinessException(MessageCode.E_SHOP_NOT_ACCEPTING_ORDER_NEXT_DAY.GetDescription(), new object[] { shop.Name });
                }
                else
                {
                    // Do nothing
                }
            }
        }
    }
}