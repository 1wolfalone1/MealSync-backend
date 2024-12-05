using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Utils;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Foods.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.UseCases.Foods.Queries.GetByIds;

public class GetByIdsForCartHandler : IQueryHandler<GetByIdsForCartQuery, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IOptionGroupRepository _optionGroupRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IFoodOptionGroupRepository _foodOptionGroupRepository;

    public GetByIdsForCartHandler(
        IFoodRepository foodRepository, IOptionGroupRepository optionGroupRepository,
        IOperatingSlotRepository operatingSlotRepository, IShopRepository shopRepository,
        IFoodOptionGroupRepository foodOptionGroupRepository)
    {
        _foodRepository = foodRepository;
        _optionGroupRepository = optionGroupRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _shopRepository = shopRepository;
        _foodOptionGroupRepository = foodOptionGroupRepository;
    }

    public async Task<Result<Result>> Handle(GetByIdsForCartQuery request, CancellationToken cancellationToken)
    {
        var foodCartCheckResponse = new FoodCartCheckResponse();
        var shop = _shopRepository.GetById(request.ShopId)!;
        var shopOperatingSlot = await _operatingSlotRepository.GetActiveByIdAndShopId(request.OperatingSlotId, request.ShopId)
            .ConfigureAwait(false);

        if (
            shop.Status == ShopStatus.Banning
            || shop.Status == ShopStatus.Banned
            || shop.Status == ShopStatus.Deleted
            || shop.Status == ShopStatus.UnApprove
        )
        {
            foodCartCheckResponse.IdsRequest = request.Foods.Select(f => f.Id).ToList();
            foodCartCheckResponse.IsRemoveAllCart = true;
            foodCartCheckResponse.IsReceivingOrderPaused = false;
            foodCartCheckResponse.IsAcceptingOrderToday = false;
            foodCartCheckResponse.IsAcceptingOrderTomorrow = false;
            foodCartCheckResponse.MessageForAllCart = "Cửa hàng đã không còn hoạt động.";
            foodCartCheckResponse.IsPresentFoodNeedRemoveToday = false;
            foodCartCheckResponse.IdsNotFoundToday = default;
            foodCartCheckResponse.IsPresentFoodNeedRemoveTomorrow = false;
            foodCartCheckResponse.IdsNotFoundTomorrow = default;
            foodCartCheckResponse.IdsRemoveAll = default;
            foodCartCheckResponse.Foods = default;
        }
        else if (shopOperatingSlot == default)
        {
            foodCartCheckResponse.IdsRequest = request.Foods.Select(f => f.Id).ToList();
            foodCartCheckResponse.IsRemoveAllCart = true;
            foodCartCheckResponse.IsReceivingOrderPaused = false;
            foodCartCheckResponse.IsAcceptingOrderToday = false;
            foodCartCheckResponse.IsAcceptingOrderTomorrow = false;
            foodCartCheckResponse.MessageForAllCart = "Cửa hàng đã không còn bán trong khung giờ này.";
            foodCartCheckResponse.IsPresentFoodNeedRemoveToday = false;
            foodCartCheckResponse.IdsNotFoundToday = default;
            foodCartCheckResponse.IsPresentFoodNeedRemoveTomorrow = false;
            foodCartCheckResponse.IdsNotFoundTomorrow = default;
            foodCartCheckResponse.IdsRemoveAll = default;
            foodCartCheckResponse.Foods = default;
        }
        else if (shop.Status == ShopStatus.InActive)
        {
            foodCartCheckResponse.IdsRequest = request.Foods.Select(f => f.Id).ToList();
            foodCartCheckResponse.IsRemoveAllCart = false;
            foodCartCheckResponse.IsReceivingOrderPaused = true;
            foodCartCheckResponse.IsAcceptingOrderToday = false;
            foodCartCheckResponse.IsAcceptingOrderTomorrow = false;
            foodCartCheckResponse.MessageForAllCart = "Cửa hàng đã đóng cửa, bạn không thể đặt đồ ăn vào bây giờ.";
            foodCartCheckResponse.IsPresentFoodNeedRemoveToday = false;
            foodCartCheckResponse.IdsNotFoundToday = default;
            foodCartCheckResponse.IsPresentFoodNeedRemoveTomorrow = false;
            foodCartCheckResponse.IdsNotFoundTomorrow = default;
            foodCartCheckResponse.IdsRemoveAll = default;
            foodCartCheckResponse.Foods = default;
        }
        else if (shop.IsAcceptingOrderNextDay)
        {
            var (foodsResponses, idsNotFoundList, namesNotFoundList, idsIsSoldOutList, namesIsSoldOutList) =
                await GetFoodsResponse(request, shopOperatingSlot).ConfigureAwait(false);
            var foodsNotOrderForTodayList = idsNotFoundList.Concat(idsIsSoldOutList).ToList();
            var namesNotOrderForTodayList = namesNotFoundList.Concat(namesIsSoldOutList).Distinct().ToList();

            if (!shop.IsReceivingOrderPaused && !shopOperatingSlot.IsReceivingOrderPaused)
            {
                // Can order for today and tomorrow
                foodCartCheckResponse.IdsRequest = request.Foods.Select(f => f.Id).ToList();
                foodCartCheckResponse.IsRemoveAllCart = false;
                foodCartCheckResponse.IsReceivingOrderPaused = false;
                foodCartCheckResponse.IsAcceptingOrderToday = true;
                foodCartCheckResponse.IsAcceptingOrderTomorrow = true;
                foodCartCheckResponse.MessageForAllCart = default;
                foodCartCheckResponse.IsPresentFoodNeedRemoveToday = foodsNotOrderForTodayList.Count > 0;
                foodCartCheckResponse.IdsNotFoundToday = foodsNotOrderForTodayList;
                foodCartCheckResponse.MessageFoodNeedRemoveToday =
                    foodsNotOrderForTodayList.Count > 0 ? $"Những món sau đây không còn hoạt động hoặc có lựa chọn hết hàng và đã bị xóa khỏi giỏ: {string.Join(", ", namesNotOrderForTodayList)}." : default;
                foodCartCheckResponse.IsPresentFoodNeedRemoveTomorrow = idsNotFoundList.Count > 0;
                foodCartCheckResponse.IdsNotFoundTomorrow = idsNotFoundList;
                foodCartCheckResponse.MessageFoodNeedRemoveTomorrow =
                    idsNotFoundList.Count > 0 ? $"Những món sau đây không còn hoạt động hoặc có lựa chọn hết hàng và đã bị xóa khỏi giỏ: {string.Join(", ", namesNotFoundList.Distinct().ToList())}." : default;
                foodCartCheckResponse.IdsRemoveAll = idsNotFoundList;
                foodCartCheckResponse.Foods = foodsResponses;
            }
            else
            {
                // Only can order for tomorrow
                foodCartCheckResponse.IdsRequest = request.Foods.Select(f => f.Id).ToList();
                foodCartCheckResponse.IsRemoveAllCart = false;
                foodCartCheckResponse.IsReceivingOrderPaused = false;
                foodCartCheckResponse.IsAcceptingOrderToday = false;
                foodCartCheckResponse.IsAcceptingOrderTomorrow = true;
                foodCartCheckResponse.MessageForAllCart = default;
                foodCartCheckResponse.IsPresentFoodNeedRemoveToday = false;
                foodCartCheckResponse.IdsNotFoundToday = default;
                foodCartCheckResponse.MessageFoodNeedRemoveToday = "Cửa hàng đã ngưng nhận đơn cho ngày hôm nay, bạn chỉ có thể đặt đơn cho ngày mai.";
                foodCartCheckResponse.IsPresentFoodNeedRemoveTomorrow = idsNotFoundList.Count > 0;
                foodCartCheckResponse.IdsNotFoundTomorrow = idsNotFoundList;
                foodCartCheckResponse.MessageFoodNeedRemoveTomorrow =
                    idsNotFoundList.Count > 0 ? $"Những món sau đây không còn hoạt động hoặc có lựa chọn hết hàng và đã bị xóa khỏi giỏ: {string.Join(", ", namesNotFoundList.Distinct().ToList())}." : default;
                foodCartCheckResponse.IdsRemoveAll = idsNotFoundList;
                foodCartCheckResponse.Foods = foodsResponses;
            }
        }
        else
        {
            var (foodsResponses, idsNotFoundList, namesNotFoundList, idsIsSoldOutList, namesIsSoldOutList) =
                await GetFoodsResponse(request, shopOperatingSlot).ConfigureAwait(false);
            var foodsNotOrderForTodayList = idsNotFoundList.Concat(idsIsSoldOutList).ToList();
            var namesNotOrderForTodayList = namesNotFoundList.Concat(namesIsSoldOutList).Distinct().ToList();

            if (!shop.IsReceivingOrderPaused && !shopOperatingSlot.IsReceivingOrderPaused)
            {
                // Only can order for today
                foodCartCheckResponse.IdsRequest = request.Foods.Select(f => f.Id).ToList();
                foodCartCheckResponse.IsRemoveAllCart = false;
                foodCartCheckResponse.IsReceivingOrderPaused = false;
                foodCartCheckResponse.IsAcceptingOrderToday = true;
                foodCartCheckResponse.IsAcceptingOrderTomorrow = false;
                foodCartCheckResponse.MessageForAllCart = default;
                foodCartCheckResponse.IsPresentFoodNeedRemoveToday = foodsNotOrderForTodayList.Count > 0;
                foodCartCheckResponse.IdsNotFoundToday = foodsNotOrderForTodayList;
                foodCartCheckResponse.MessageFoodNeedRemoveToday =
                    foodsNotOrderForTodayList.Count > 0 ? $"Những món sau đây không còn hoạt động hoặc có lựa chọn hết hàng và đã bị xóa khỏi giỏ: {string.Join(", ", namesNotOrderForTodayList)}." : default;
                foodCartCheckResponse.IsPresentFoodNeedRemoveTomorrow = false;
                foodCartCheckResponse.IdsNotFoundTomorrow = default;
                foodCartCheckResponse.MessageFoodNeedRemoveTomorrow = "Cửa hàng không nhận đơn cho ngày mai, bạn chỉ có thể đặt đơn trong ngày hôm nay.";
                foodCartCheckResponse.IdsRemoveAll = idsNotFoundList;
                foodCartCheckResponse.Foods = foodsResponses;
            }
            else
            {
                // Can't order for today or tomorrow
                foodCartCheckResponse.IdsRequest = request.Foods.Select(f => f.Id).ToList();
                foodCartCheckResponse.IsRemoveAllCart = false;
                foodCartCheckResponse.IsReceivingOrderPaused = true;
                foodCartCheckResponse.IsAcceptingOrderToday = false;
                foodCartCheckResponse.IsAcceptingOrderTomorrow = false;
                foodCartCheckResponse.MessageForAllCart = "Cửa hàng đã ngưng nhận đơn ngày hôm nay và không nhận đặt đơn cho ngày mai.";
                foodCartCheckResponse.IsPresentFoodNeedRemoveToday = false;
                foodCartCheckResponse.IdsNotFoundToday = default;
                foodCartCheckResponse.MessageFoodNeedRemoveToday = default;
                foodCartCheckResponse.IsPresentFoodNeedRemoveTomorrow = false;
                foodCartCheckResponse.IdsNotFoundTomorrow = default;
                foodCartCheckResponse.MessageFoodNeedRemoveTomorrow = default;
                foodCartCheckResponse.IdsRemoveAll = default;
                foodCartCheckResponse.Foods = foodsResponses;
            }
        }

        if (shopOperatingSlot != default)
        {
            var now = TimeFrameUtils.GetCurrentDateInUTC7();
            var endTimeInMinutes = TimeUtils.ConvertToMinutes(shopOperatingSlot.EndTime);
            var currentTimeMinutes = (now.Hour * 60) + now.Minute;
            if (shopOperatingSlot.EndTime != 2400 && currentTimeMinutes >= endTimeInMinutes)
            {
                foodCartCheckResponse.IsAcceptingOrderToday = false;
                foodCartCheckResponse.MessageFoodNeedRemoveToday = $"Thời gian hiện tại đã vượt quá khung thời gian bán {TimeFrameUtils.GetTimeFrameString(shopOperatingSlot.StartTime, shopOperatingSlot.EndTime)}.";
            }
        }

        if (foodCartCheckResponse.IdsNotFoundToday != null && foodCartCheckResponse.IdsNotFoundToday.Count == request.Foods.Count)
        {
            foodCartCheckResponse.IsAcceptingOrderToday = false;
        }

        if (foodCartCheckResponse.IdsNotFoundTomorrow != null && foodCartCheckResponse.IdsNotFoundTomorrow.Count == request.Foods.Count)
        {
            foodCartCheckResponse.IsAcceptingOrderTomorrow = false;
        }

        return Result.Success(foodCartCheckResponse);
    }

    private async Task<(
        List<FoodCartCheckResponse.DetailFoodResponse> FoodsResponses,
        List<string> IdsNotFoundList,
        List<string> NamesNotFoundList,
        List<string> IdsIsSoldOutList,
        List<string> NamesIsSoldOutList
        )> GetFoodsResponse(GetByIdsForCartQuery request, OperatingSlot shopOperatingSlot)
    {
        var foodsResponses = new List<FoodCartCheckResponse.DetailFoodResponse>();
        var idsNotFoundList = new List<string>();
        var namesNotFoundList = new List<string>();
        var idsIsSoldOutList = new List<string>();
        var namesIsSoldOutList = new List<string>();
        foreach (var foodRequest in request.Foods)
        {
            var validId = long.TryParse(foodRequest.Id.Split('-')[0], out var id);

            if (!validId)
            {
                idsNotFoundList.Add(foodRequest.Id);
            }
            else
            {
                var foodRequestId = id;
                var food = await _foodRepository.GetActiveFood(foodRequestId, shopOperatingSlot.Id).ConfigureAwait(false);

                if (food == default)
                {
                    idsNotFoundList.Add(foodRequest.Id);
                    var foodName = await _foodRepository.GetFoodNameByIdAndShopId(foodRequestId, request.ShopId).ConfigureAwait(false);
                    if (!string.IsNullOrEmpty(foodName))
                    {
                        namesNotFoundList.Add(foodName);
                    }
                }
                else
                {
                    var foodResponse = new FoodCartCheckResponse.DetailFoodResponse
                    {
                        Id = foodRequest.Id,
                        Name = food.Name,
                        Description = food.Description,
                        Price = food.Price,
                        ImageUrl = food.ImageUrl,
                        ShopId = food.ShopId,
                    };
                    bool isNotFound = false;
                    var idsOptionGroupRequired = await _foodOptionGroupRepository.GetAllIdsRequiredByFoodId(food.Id).ConfigureAwait(false);
                    var idsOptionGroupCheck = new List<long>();

                    if (foodRequest.OptionGroupRadio != default && foodRequest.OptionGroupRadio.Count > 0 && !isNotFound)
                    {
                        var optionGroupRadioResponses = new List<FoodCartCheckResponse.OptionGroupRadioResponse>();

                        foreach (var optionGroupRequest in foodRequest.OptionGroupRadio)
                        {
                            idsOptionGroupCheck.Add(optionGroupRequest.Id);
                            var optionGroup = await _optionGroupRepository.GetByIdAndOptionIds(
                                optionGroupRequest.Id, new[] { optionGroupRequest.OptionId }
                            ).ConfigureAwait(false);

                            if (optionGroup == default || !optionGroup.Options.Select(o => o.Id).Contains(optionGroupRequest.OptionId))
                            {
                                idsNotFoundList.Add(foodRequest.Id);
                                isNotFound = true;
                                break;
                            }
                            else
                            {
                                var option = optionGroup.Options.First();
                                optionGroupRadioResponses.Add(new FoodCartCheckResponse.OptionGroupRadioResponse
                                {
                                    Id = optionGroup.Id,
                                    Title = optionGroup.Title,
                                    Option = new FoodCartCheckResponse.OptionResponse
                                    {
                                        Id = option.Id,
                                        Title = option.Title,
                                        ImageUrl = option.ImageUrl,
                                        Price = option.Price,
                                        IsCalculatePrice = option.IsCalculatePrice,
                                    },
                                });
                            }
                        }

                        foodResponse.OptionGroupRadio = optionGroupRadioResponses;
                    }

                    if (foodRequest.OptionGroupCheckbox != default && foodRequest.OptionGroupCheckbox.Count > 0 && !isNotFound)
                    {
                        var optionGroupCheckboxResponses = new List<FoodCartCheckResponse.OptionGroupCheckboxResponse>();

                        foreach (var optionGroupRequest in foodRequest.OptionGroupCheckbox)
                        {
                            idsOptionGroupCheck.Add(optionGroupRequest.Id);
                            var optionGroup = await _optionGroupRepository.GetByIdAndOptionIds(
                                optionGroupRequest.Id, optionGroupRequest.OptionIds
                            ).ConfigureAwait(false);
                            var availableOptionIds = optionGroup?.Options.Select(o => o.Id).ToList() ?? new List<long>();

                            if (optionGroup == default || !optionGroupRequest.OptionIds.All(optionId => availableOptionIds.Contains(optionId)))
                            {
                                idsNotFoundList.Add(foodRequest.Id);
                                isNotFound = true;
                                break;
                            }
                            else if (!await _optionGroupRepository.CheckMinMaxChoice(optionGroup.Id, optionGroupRequest.OptionIds.Length).ConfigureAwait(false))
                            {
                                idsNotFoundList.Add(foodRequest.Id);
                                isNotFound = true;
                                break;
                            }
                            else
                            {
                                var matchingOptions = optionGroup.Options
                                    .Where(o => optionGroupRequest.OptionIds.Contains(o.Id))
                                    .Select(o => new FoodCartCheckResponse.OptionResponse
                                    {
                                        Id = o.Id,
                                        Title = o.Title,
                                        ImageUrl = o.ImageUrl,
                                        Price = o.Price,
                                        IsCalculatePrice = o.IsCalculatePrice,
                                    })
                                    .ToList();

                                var optionGroupCheckboxResponse = new FoodCartCheckResponse.OptionGroupCheckboxResponse
                                {
                                    Id = optionGroup.Id,
                                    Title = optionGroup.Title,
                                    Options = matchingOptions,
                                };

                                optionGroupCheckboxResponses.Add(optionGroupCheckboxResponse);
                            }
                        }

                        foodResponse.OptionGroupCheckbox = optionGroupCheckboxResponses;
                    }

                    bool allIdsContained = idsOptionGroupRequired.All(ogId => idsOptionGroupCheck.Contains(ogId));
                    if (!allIdsContained)
                    {
                        idsNotFoundList.Add(foodRequest.Id);
                        isNotFound = true;
                    }

                    if (isNotFound)
                    {
                        var foodName = await _foodRepository.GetFoodNameByIdAndShopId(foodRequestId, request.ShopId).ConfigureAwait(false);
                        if (!string.IsNullOrEmpty(foodName))
                        {
                            namesNotFoundList.Add(foodName);
                        }
                    }
                    else
                    {
                        foodsResponses.Add(foodResponse);
                        if (food.IsSoldOut)
                        {
                            idsIsSoldOutList.Add(foodRequest.Id);
                            var foodName = await _foodRepository.GetFoodNameByIdAndShopId(foodRequestId, request.ShopId).ConfigureAwait(false);
                            if (!string.IsNullOrEmpty(foodName))
                            {
                                namesIsSoldOutList.Add(foodName);
                            }
                        }
                    }
                }
            }
        }

        return (foodsResponses, idsNotFoundList, namesNotFoundList, idsIsSoldOutList, namesIsSoldOutList);
    }
}