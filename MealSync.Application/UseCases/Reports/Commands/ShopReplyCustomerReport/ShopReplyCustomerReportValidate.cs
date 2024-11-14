using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace MealSync.Application.UseCases.Reports.Commands.ShopReplyCustomerReport;

public class ShopReplyCustomerReportValidate : AbstractValidator<ShopReplyCustomerReportCommand>
{
    public ShopReplyCustomerReportValidate()
    {
        RuleFor(x => x.ReplyReportId)
            .GreaterThan(0).WithMessage("Id phải lớn hơn 0");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Tiêu đề báo cáo đơn hàng không được để trống")
            .MaximumLength(400)
            .WithMessage("Tiêu đề báo cáo tối đa 400 kí tự");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Nội dung báo cáo đơn hàng không được để trống")
            .MaximumLength(800)
            .WithMessage("Nội dung báo cáo tối đa 800 kí tự.");

        RuleFor(x => x.Images)
            .Must(images => images != default && images.Count > 0 && images.Count <= 5)
            .WithMessage("Ảnh báo cáo phải có ít nhất 1 và tối đa 5");
    }
}