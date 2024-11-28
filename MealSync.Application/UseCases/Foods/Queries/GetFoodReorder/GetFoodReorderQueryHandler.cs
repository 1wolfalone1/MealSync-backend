using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Application.UseCases.Shops.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Foods.Queries.GetFoodReorder;

public class GetFoodReorderQueryHandler : IQueryHandler<GetFoodReorderQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IOrderRepository _orderRepository;
    private readonly IShopDormitoryRepository _shopDormitoryRepository;
    private readonly IBuildingRepository _buildingRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IFoodRepository _foodRepository;
    private readonly IOptionRepository _optionRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IMapper _mapper;

    public GetFoodReorderQueryHandler(
        ICurrentPrincipalService currentPrincipalService, IOrderRepository orderRepository,
        IShopDormitoryRepository shopDormitoryRepository, IBuildingRepository buildingRepository,
        IShopRepository shopRepository, IFoodRepository foodRepository, IOptionRepository optionRepository,
        IOperatingSlotRepository operatingSlotRepository, IMapper mapper,
        IFoodOptionGroupRepository foodOptionGroupRepository, IOptionGroupRepository optionGroupRepository)
    {
        _currentPrincipalService = currentPrincipalService;
        _orderRepository = orderRepository;
        _shopDormitoryRepository = shopDormitoryRepository;
        _buildingRepository = buildingRepository;
        _shopRepository = shopRepository;
        _foodRepository = foodRepository;
        _optionRepository = optionRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _mapper = mapper;
        _foodOptionGroupRepository = foodOptionGroupRepository;
        _optionGroupRepository = optionGroupRepository;
    }

    public async Task<Result<Result>> Handle(GetFoodReorderQuery request, CancellationToken cancellationToken)
    {
        var customerId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var order = await _orderRepository.GetByIdAndCustomerIdForReorder(request.OrderId, customerId).ConfigureAwait(false);
        var now = TimeFrameUtils.GetCurrentDateInUTC7();
        Building? buildingOrder = default;
        if (!request.IsGetShopInfo && request.BuildingOrderId.HasValue)
        {
            buildingOrder = _buildingRepository.GetById(request.BuildingOrderId);
        }

        var foodReOrderResponse = new FoodReOrderResponse();
        if (order == default)
        {
            throw new InvalidBusinessException(MessageCode.E_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId });
        }
        else
        {
            OperatingSlot? operatingSlot = default;
            if (!request.IsGetShopInfo && request.OperatingSlotId.HasValue)
            {
                operatingSlot = await _operatingSlotRepository.GetActiveByIdAndShopId(request.OperatingSlotId.Value, order.ShopId).ConfigureAwait(false);
            }

            var shop = await _shopRepository.GetShopInfoForReOrderById(order.ShopId).ConfigureAwait(false)!;
            if (shop.Status != ShopStatus.Active)
            {
                foodReOrderResponse.IsAllowReOrder = false;
                foodReOrderResponse.MessageNotAllow = "Cửa hàng đã đóng cửa, bạn không thể đặt lại đơn hàng vào bây giờ.";
                foodReOrderResponse.Note = default;
                foodReOrderResponse.ShopInfo = default;
                foodReOrderResponse.Foods = default;
                return Result.Success(foodReOrderResponse);
            }
            else if (!request.IsGetShopInfo && operatingSlot == default)
            {
                foodReOrderResponse.IsAllowReOrder = false;
                foodReOrderResponse.MessageNotAllow = "Cửa hàng đã không còn bán trong khung giờ này.";
                foodReOrderResponse.Note = default;
                foodReOrderResponse.ShopInfo = default;
                foodReOrderResponse.Foods = default;
                return Result.Success(foodReOrderResponse);
            }
            else if (!request.IsGetShopInfo && !request.IsOrderForNextDay!.Value && (shop.IsReceivingOrderPaused || operatingSlot.IsReceivingOrderPaused))
            {
                foodReOrderResponse.IsAllowReOrder = false;
                foodReOrderResponse.MessageNotAllow = "Cửa hàng đã ngưng nhận đơn cho ngày hôm nay.";
                foodReOrderResponse.Note = default;
                foodReOrderResponse.ShopInfo = default;
                foodReOrderResponse.Foods = default;
                return Result.Success(foodReOrderResponse);
            }
            else if (!request.IsGetShopInfo && request.IsOrderForNextDay!.Value && !shop.IsAcceptingOrderNextDay)
            {
                foodReOrderResponse.IsAllowReOrder = false;
                foodReOrderResponse.MessageNotAllow = "Cửa hàng không nhận đặt hàng cho ngày mai.";
                foodReOrderResponse.Note = default;
                foodReOrderResponse.ShopInfo = default;
                foodReOrderResponse.Foods = default;
                return Result.Success(foodReOrderResponse);
            }
            else if (!request.IsGetShopInfo && buildingOrder != default && !await _shopDormitoryRepository.CheckExistedByShopIdAndDormitoryId(order.ShopId, buildingOrder.DormitoryId).ConfigureAwait(false))
            {
                foodReOrderResponse.IsAllowReOrder = false;
                foodReOrderResponse.MessageNotAllow = $"Cửa hàng không còn nhận đặt hàng tại {buildingOrder.Name}.";
                foodReOrderResponse.Note = default;
                foodReOrderResponse.ShopInfo = default;
                foodReOrderResponse.Foods = default;
                return Result.Success(foodReOrderResponse);
            }
            else
            {
                var isNotAllowReorder = false;
                bool isAllFoodActive;
                var foodIds = new List<long>();
                var optionIds = new List<long>();

                foreach (var orderDetail in order.OrderDetails)
                {
                    foodIds.Add(orderDetail.FoodId);
                    optionIds.AddRange(orderDetail.OrderDetailOptions.Select(odo => odo.OptionId));
                }

                if (!request.IsGetShopInfo)
                {
                    isAllFoodActive = await _foodRepository.CheckActiveFoodByIds(foodIds, operatingSlot!.Id).ConfigureAwait(false);
                }
                else
                {
                    isAllFoodActive = await _foodRepository.CheckActiveFoodByIds(foodIds).ConfigureAwait(false);
                }

                var isAllOptionActive = await _optionRepository.CheckAllOptionAndOptionGroupActiveByIds(optionIds).ConfigureAwait(false);

                if (isAllFoodActive && isAllOptionActive)
                {
                    foodReOrderResponse.IsAllowReOrder = true;
                    foodReOrderResponse.MessageNotAllow = default;
                    foodReOrderResponse.Note = order.Note;

                    var foods = new List<FoodReOrderResponse.FoodDetailReorderResponse>();
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        var idsOptionGroupRequired = await _foodOptionGroupRepository.GetAllIdsRequiredByFoodId(orderDetail.FoodId).ConfigureAwait(false);
                        var idsOptionGroupCheck = new List<long>();
                        var optionsGroupCheckbox = new List<FoodReOrderResponse.OptionGroupCheckboxReOrderResponse>();
                        var optionsGroupRadio = new List<FoodReOrderResponse.OptionGroupRadioReOrderResponse>();
                        var food = new FoodReOrderResponse.FoodDetailReorderResponse();
                        food.Id = orderDetail.FoodId;
                        food.Quantity = orderDetail.Quantity;
                        food.Name = orderDetail.Food.Name;
                        food.Description = orderDetail.Food.Description;
                        food.Price = orderDetail.Food.Price;
                        food.ImageUrl = orderDetail.Food.ImageUrl;
                        foreach (var optionDetail in orderDetail.OrderDetailOptions)
                        {
                            var option = await _optionRepository.GetIncludeOptionGroupById(optionDetail.OptionId).ConfigureAwait(false);
                            if (option.OptionGroup.Type == OptionGroupTypes.Radio)
                            {
                                idsOptionGroupCheck.Add(option.OptionGroupId);
                                var optionGroupRadio = new FoodReOrderResponse.OptionGroupRadioReOrderResponse
                                {
                                    Id = option.OptionGroupId,
                                    Title = option.OptionGroup.Title,
                                    Option = new FoodReOrderResponse.OptionReOrderResponse
                                    {
                                        Id = option.Id,
                                        Title = option.Title,
                                        Price = option.Price,
                                        ImageUrl = option.ImageUrl,
                                        IsCalculatePrice = option.IsCalculatePrice,
                                    },
                                };
                                optionsGroupRadio.Add(optionGroupRadio);
                            }
                            else
                            {
                                var existingCheckboxGroup = optionsGroupCheckbox
                                    .FirstOrDefault(og => og.Id == option.OptionGroupId);

                                if (existingCheckboxGroup != default)
                                {
                                    existingCheckboxGroup.Options.Add(new FoodReOrderResponse.OptionReOrderResponse
                                    {
                                        Id = option.Id,
                                        Title = option.Title,
                                        Price = option.Price,
                                        ImageUrl = option.ImageUrl,
                                        IsCalculatePrice = option.IsCalculatePrice,
                                    });
                                }
                                else
                                {
                                    idsOptionGroupCheck.Add(option.OptionGroupId);
                                    var optionGroupCheckbox = new FoodReOrderResponse.OptionGroupCheckboxReOrderResponse
                                    {
                                        Id = option.OptionGroupId,
                                        Title = option.OptionGroup.Title,
                                        Options = new List<FoodReOrderResponse.OptionReOrderResponse>
                                        {
                                            new FoodReOrderResponse.OptionReOrderResponse
                                            {
                                                Id = option.Id,
                                                Title = option.Title,
                                                Price = option.Price,
                                                ImageUrl = option.ImageUrl,
                                                IsCalculatePrice = option.IsCalculatePrice,
                                            },
                                        },
                                    };
                                    optionsGroupCheckbox.Add(optionGroupCheckbox);
                                }
                            }
                        }

                        bool allIdsContained = idsOptionGroupRequired.All(ogId => idsOptionGroupCheck.Contains(ogId));
                        if (!allIdsContained)
                        {
                            isNotAllowReorder = true;
                            break;
                        }

                        // Check min choice, max choice
                        foreach (var optionGroup in optionsGroupCheckbox)
                        {
                            var isQualifiedMinMaxChoice = await _optionGroupRepository.CheckMinMaxChoice(optionGroup.Id, optionGroup.Options.Count).ConfigureAwait(false);
                            if (!isQualifiedMinMaxChoice)
                            {
                                isNotAllowReorder = true;
                                break;
                            }
                        }

                        if (isNotAllowReorder)
                        {
                            break;
                        }

                        food.OptionGroupCheckbox = optionsGroupCheckbox;
                        food.OptionGroupRadio = optionsGroupRadio;
                        foods.Add(food);
                    }

                    if (isNotAllowReorder)
                    {
                        foodReOrderResponse.IsAllowReOrder = false;
                        foodReOrderResponse.MessageNotAllow = "Cửa hàng không còn bán một số thức ăn/đồ uống trong đơn hàng này.";
                        foodReOrderResponse.Note = default;
                        foodReOrderResponse.ShopInfo = default;
                        foodReOrderResponse.Foods = default;
                        return Result.Success(foodReOrderResponse);
                    }
                    else
                    {
                        if (request.IsGetShopInfo)
                        {
                            foodReOrderResponse.Note = default;
                            foodReOrderResponse.Foods = default;
                        }
                        else
                        {
                            foodReOrderResponse.Foods = foods;
                        }

                        var shopInfoReOrderResponse = new FoodReOrderResponse.ShopInfoReOrderResponse();
                        var operatingSlotReOrderResponses = new List<FoodReOrderResponse.OperatingSlotReOrderResponse>();
                        foreach (var slot in shop.OperatingSlots)
                        {
                            var endTimeInMinutes = TimeUtils.ConvertToMinutes(slot.EndTime);
                            var currentTimeMinutes = (now.Hour * 60) + now.Minute;
                            var operatingSlotReOrderResponse = new FoodReOrderResponse.OperatingSlotReOrderResponse
                            {
                                Id = slot.Id,
                                Title = slot.Title,
                                StartTime = slot.StartTime,
                                EndTime = slot.EndTime,
                                IsAcceptingOrderTomorrow = shop.IsAcceptingOrderNextDay,
                                IsAcceptingOrderToday = !shop.IsReceivingOrderPaused && !slot.IsReceivingOrderPaused && (slot.EndTime == 2400 || currentTimeMinutes < endTimeInMinutes),
                            };
                            operatingSlotReOrderResponses.Add(operatingSlotReOrderResponse);
                        }

                        shopInfoReOrderResponse.Id = shop.Id;
                        shopInfoReOrderResponse.Name = shop.Name;
                        shopInfoReOrderResponse.LogoUrl = shop.LogoUrl;
                        shopInfoReOrderResponse.BannerUrl = shop.BannerUrl;
                        shopInfoReOrderResponse.Description = shop.Description;
                        shopInfoReOrderResponse.Location = _mapper.Map<FoodReOrderResponse.LocationResponse>(shop.Location);
                        shopInfoReOrderResponse.Dormitories = _mapper.Map<List<FoodReOrderResponse.DormitoryReOrderResponse>>(shop.ShopDormitories.Select(sd => sd.Dormitory));
                        shopInfoReOrderResponse.OperatingSlots = operatingSlotReOrderResponses;

                        foodReOrderResponse.ShopInfo = shopInfoReOrderResponse;
                        return Result.Success(foodReOrderResponse);
                    }
                }
                else
                {
                    foodReOrderResponse.IsAllowReOrder = false;
                    foodReOrderResponse.MessageNotAllow = "Cửa hàng không còn bán một số thức ăn/đồ uống trong đơn hàng này.";
                    foodReOrderResponse.Note = default;
                    foodReOrderResponse.ShopInfo = default;
                    foodReOrderResponse.Foods = default;
                    return Result.Success(foodReOrderResponse);
                }
            }
        }
    }
}