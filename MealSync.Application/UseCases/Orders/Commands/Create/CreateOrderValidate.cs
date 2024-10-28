using FluentValidation;
using MealSync.Application.Common.Constants;
using MealSync.Application.Common.Utils;

namespace MealSync.Application.UseCases.Orders.Commands.Create;

public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderValidator()
    {
        // Shop validation
        RuleFor(x => x.ShopId)
            .GreaterThan(0).WithMessage("Shop id phải lớn hơn 0.");

        // Full name validation
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Tên người đặt hàng bắt buộc.");

        // Phone number validation
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại người đặt hàng bắt buộc.");

        // Building validation
        RuleFor(x => x.BuildingId)
            .GreaterThan(0).WithMessage("Tòa nhà id phải lớn hơn 0.");

        // Foods validation
        RuleFor(x => x.Foods)
            .NotEmpty().WithMessage("Phải có ít nhất một sản phẩm.")
            .ForEach(product =>
            {
                product.SetValidator(new FoodOrderValidator());
            });

        // Order TimeFrame validation
        RuleFor(x => x.OrderTime)
            .SetValidator(new OrderTimeFrameValidator());

        // Total Discount validation
        RuleFor(x => x.TotalDiscount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Tổng giảm giá không được nhỏ hơn 0.")
            .LessThanOrEqualTo(x => x.TotalFoodCost)
            .WithMessage("Tiền giảm giá phải nhỏ hơn hoặc bằng tổng tiền đồ ăn.");;

        // Total Food Cost validation
        RuleFor(x => x.TotalFoodCost)
            .GreaterThan(0).WithMessage("Tổng tiền đồ ăn phải lớn hơn 0.");

        // Total Order validation (Total order should be equal to or greater than food cost minus discounts)
        RuleFor(x => x.TotalOrder)
            .Equal(x => x.TotalFoodCost - x.TotalDiscount)
            .WithMessage("Tổng đơn hàng phải bằng tổng tiền đồ ăn trừ đi giảm giá.");

        // Payment method validation
        RuleFor(x => x.PaymentMethod)
            .IsInEnum()
            .WithMessage("Payment method phải là VnPay (1) hoặc COD (2).");

        // Ship info validation
        RuleFor(x => x.ShipInfo)
            .SetValidator(new ShipInfoValidator());
    }

    // Validator for FoodOrderCommand
    public class FoodOrderValidator : AbstractValidator<CreateOrderCommand.FoodOrderCommand>
    {
        public FoodOrderValidator()
        {
            // Food ID validation
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Đồ ăn id bắt buộc.")
                .Must(id => id.Contains("-"))
                .WithMessage("Đồ ăn id gửi không đúng định dạng.")
                .Must(id => id.Split('-').Length > 1 && long.TryParse(id.Split('-')[0], out var productId) && productId > 0)
                .WithMessage("Đồ ăn id phải lớn hơn 0.");

            // Quantity validation
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Số lượng phải lớn hơn 0.");

            // OptionGroupRadio validation
            RuleForEach(x => x.OptionGroupRadio)
                .SetValidator(new OptionGroupRadioValidator());

            // OptionGroupCheckbox validation
            RuleForEach(x => x.OptionGroupCheckbox)
                .SetValidator(new OptionGroupCheckboxValidator());
        }
    }

    // Validator for OptionGroupRadioCommand
    public class OptionGroupRadioValidator : AbstractValidator<CreateOrderCommand.OptionGroupRadioCommand>
    {
        public OptionGroupRadioValidator()
        {
            // Group ID validation
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Lựa chọn nhóm id phải lớn hơn 0.");

            // Option validation
            RuleFor(x => x.OptionId)
                .GreaterThan(0).WithMessage("Lựa chọn id phải lớn hơn 0.");
        }
    }

    // Validator for OptionGroupCheckboxCommand
    public class OptionGroupCheckboxValidator : AbstractValidator<CreateOrderCommand.OptionGroupCheckboxCommand>
    {
        public OptionGroupCheckboxValidator()
        {
            // Group ID validation
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Lựa chọn nhóm id phải lớn hơn 0.");

            // Option validation
            RuleFor(x => x.OptionIds)
                .NotEmpty().WithMessage("Phải có ít nhất một lựa chọn.");

            RuleForEach(x => x.OptionIds)
                .GreaterThan(0).WithMessage("Mỗi lựa chọn id phải lớn hơn 0.");
        }
    }

    // Validator for OrderTimeFrame
    public class OrderTimeFrameValidator : AbstractValidator<CreateOrderCommand.OrderTimeFrame>
    {
        public OrderTimeFrameValidator()
        {
            // Validate start and end times (within 24 hours, 0-23 range)
            RuleFor(x => x.StartTime)
                .Must(TimeUtils.IsValidOperatingSlot)
                .WithMessage("Vui lòng cung cấp thời gian bắt đầu đúng định dạng hhMM.");

            RuleFor(x => x)
                .Must(x => x.EndTime > x.StartTime && TimeUtils.IsValidOperatingSlot(x.EndTime) && TimeUtils.IsThirtyMinuteDifference(x.StartTime, x.EndTime))
                .WithMessage($"Thời gian kết thúc bằng thời gian bắt đầu cộng {FrameConstant.TIME_FRAME_IN_MINUTES} phút.");

            // Future orders validation (if order is for the next day)
            RuleFor(x => x.IsOrderNextDay)
                .NotNull().WithMessage("Phải xác định rõ có phải là đơn hàng cho ngày tiếp theo hay không.");
        }
    }

    // Validator for ShipInfo
    public class ShipInfoValidator : AbstractValidator<CreateOrderCommand.ShipInfoCommand>
    {
        public ShipInfoValidator()
        {
            // Validate duration
            RuleFor(x => x.Duration)
                .GreaterThan(0)
                .WithMessage("Thời gian giao hàng phải lớn hơn 0.");

            // Validate distance
            RuleFor(x => x.Distance)
                .GreaterThan(0)
                .WithMessage("Khoảng cách giao hàng phải lớn hơn 0.");
        }
    }
}