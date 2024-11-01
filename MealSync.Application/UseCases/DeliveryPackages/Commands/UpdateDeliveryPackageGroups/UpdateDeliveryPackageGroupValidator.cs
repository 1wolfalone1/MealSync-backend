using FluentValidation;

namespace MealSync.Application.UseCases.DeliveryPackages.Commands.UpdateDeliveryPackageGroups;

public class UpdateDeliveryPackageGroupValidator : AbstractValidator<UpdateDeliveryPackageGroupCommand>
{
    public UpdateDeliveryPackageGroupValidator()
    {
        RuleFor(x => x.IsConfirm)
            .NotNull()
            .WithMessage("Vui lòng xác nhận bạn có muốn bỏ qua cảnh báo hay không");

        RuleFor(x => x.DeliveryPackages)
            .NotEmpty()
            .WithMessage("Vui lòng cung cấp ít nhất 1 gói hàng ");

        RuleFor(command => command.DeliveryPackages)
            .Must(NoDuplicateShopDeliveryStaffIds)
            .WithMessage("Không thể tồn tại 1 nhân viên hoặc chủ cửa hàng có mặt trong nhiều hơn 2 gói hàng");

        RuleFor(command => command.DeliveryPackages)
            .Must(NoDuplicateOrderIds)
            .WithMessage("Không thể tồn tại 1 đơn hàng có mặt trong nhiều hơn 2 gói hàng");

        RuleFor(command => command.DeliveryPackages)
            .Must(NoDuplicateDeliveryPackageIds)
            .WithMessage("Vui lòng không truyền trùng deliveryPackageId về");

        RuleForEach(x => x.DeliveryPackages)
            .SetValidator(new DeliveryPackageRequestValidator());
    }

    private bool NoDuplicateShopDeliveryStaffIds(List<UpdateDeliveryPackageGroupCommand.DeliveryPackageInUpdateRequest> packages)
    {
        // Check shop owner delivery two package
        if (packages.Where(p => !p.ShopDeliveryStaffId.HasValue).Count() > 1)
            return false;

        var shopDeliveryStaffIds = packages
            .Where(p => p.ShopDeliveryStaffId.HasValue)
            .Select(p => p.ShopDeliveryStaffId.Value);

        return shopDeliveryStaffIds.Distinct().Count() == shopDeliveryStaffIds.Count();
    }

    // Check for duplicate OrderIds across all packages
    private bool NoDuplicateOrderIds(List<UpdateDeliveryPackageGroupCommand.DeliveryPackageInUpdateRequest> packages)
    {
        var allOrderIds = packages
            .SelectMany(p => p.OrderIds);

        return allOrderIds.Distinct().Count() == allOrderIds.Count();
    }

    private bool NoDuplicateDeliveryPackageIds(List<UpdateDeliveryPackageGroupCommand.DeliveryPackageInUpdateRequest> packages)
    {
        var deliveryPackageIds = packages
            .Select(p => p.DeliveryPackageId);

        return deliveryPackageIds.Distinct().Count() == deliveryPackageIds.Count();
    }
}

public class DeliveryPackageRequestValidator : AbstractValidator<UpdateDeliveryPackageGroupCommand.DeliveryPackageInUpdateRequest>
{
    public DeliveryPackageRequestValidator()
    {
        When(x => x.ShopDeliveryStaffId.HasValue, (() =>
        {
            RuleFor(x => x.ShopDeliveryStaffId)
                .GreaterThan(0)
                .WithMessage("Vui lòng cung cấp id của nhân viên giao hàng. Nếu shop tự giao vui lòng xóa field shopDeliveryStaffId");
        }));

        RuleFor(x => x.OrderIds)
            .Must(x => x.Length > 0)
            .WithMessage("Phải có ít nhất 1 đơn hàng để tạo gói hàng");
    }
}