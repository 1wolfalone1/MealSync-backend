using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Orders.Commands.Create;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;
using MediatR;

namespace MealSync.Application.UseCases.Orders.Commands.CreateOrderDataSample;

public class CreateOrderDataSampleHandler : ICommandHandler<CreateOrderDataSampleCommand, Result>
{
    private readonly IFoodRepository _foodRepository;
    private readonly IOperatingSlotRepository _operatingSlotRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IMediator _mediator;

    public CreateOrderDataSampleHandler(
        IFoodRepository foodRepository, IOperatingSlotRepository operatingSlotRepository,
        IAccountRepository accountRepository, IMediator mediator)
    {
        _foodRepository = foodRepository;
        _operatingSlotRepository = operatingSlotRepository;
        _accountRepository = accountRepository;
        _mediator = mediator;
    }

    public async Task<Result<Result>> Handle(CreateOrderDataSampleCommand request, CancellationToken cancellationToken)
    {
        var shopOperatingSlot = await _operatingSlotRepository.GetAvailableForTimeRangeOrder(
                request.ShopId, request.StartTime, request.EndTime)
            .ConfigureAwait(false);

        if (shopOperatingSlot == default)
        {
            throw new InvalidBusinessException(
                MessageCode.E_ORDER_SHOP_NOT_SELL_IN_ORDER_TIME.GetDescription(),
                new object[] { request.StartTime, request.EndTime }
            );
        }

        var foods = await _foodRepository.GetActiveFood(shopOperatingSlot!.Id).ConfigureAwait(false);
        var accounts = _accountRepository.GetAccountByIds(request.CustomerIds);

        var random = new Random();

        for (int i = 0; i < accounts.Count; i++)
        {
            for (int j = 1; j <= 1; j++)
            {
                int totalFoodCost = 0;
                var foodsOrder = new List<CreateOrderCommand.FoodOrderCommand>();
                var selectedIndices = new HashSet<int>();

                while (foodsOrder.Count < 1 && foodsOrder.Count < foods.Count)
                {
                    int randomIndex = random.Next(foods.Count);

                    if (selectedIndices.Add(randomIndex))
                    {
                        var selectedFood = foods[randomIndex];
                        var foodOrderCommand = new CreateOrderCommand.FoodOrderCommand
                        {
                            Id = selectedFood.Id + "-radio:(1,1)|checkbox:(2,[3])",
                            Quantity = 1,
                        };
                        foodsOrder.Add(foodOrderCommand);
                        totalFoodCost += (int)selectedFood.Price * foodOrderCommand.Quantity;
                    }
                }

                CreateOrderCommand createOrder = new CreateOrderCommand
                {
                    IsDummy = true,
                    CustomerId = accounts[i].Id,
                    ShopId = request.ShopId,
                    FullName = accounts[i].FullName ?? "Nguyen Van A",
                    PhoneNumber = accounts[i].PhoneNumber,
                    BuildingId = i % 2 == 0 ? 9 : 25,
                    Foods = foodsOrder,
                    OrderTime = new CreateOrderCommand.OrderTimeFrame
                    {
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        IsOrderNextDay = false,
                    },
                    VoucherId = null,
                    TotalDiscount = 0,
                    TotalFoodCost = totalFoodCost,
                    TotalOrder = totalFoodCost,
                    PaymentMethod = PaymentMethods.COD,
                    ShipInfo = new CreateOrderCommand.ShipInfoCommand
                    {
                        Distance = 3.2,
                        Duration = 12,
                    },
                };

                await _mediator.Send(createOrder, cancellationToken).ConfigureAwait(false);
            }
        }

        return Result.Success("Tạo đơn hàng thành công");
    }
}