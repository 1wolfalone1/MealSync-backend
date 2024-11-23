using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reports.Queries.GetByOrderId;

public class GetByOrderIdHandler : IQueryHandler<GetByOrderIdQuery, Result>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IAccountRepository _accountRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public GetByOrderIdHandler(
        IReportRepository reportRepository, ICurrentPrincipalService currentPrincipalService,
        IAccountRepository accountRepository, IOrderRepository orderRepository, IMapper mapper)
    {
        _reportRepository = reportRepository;
        _currentPrincipalService = currentPrincipalService;
        _accountRepository = accountRepository;
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var shopId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var existed = await _reportRepository.CheckExistedByOrderIdAndShopId(request.OrderId, shopId).ConfigureAwait(false);

        if (existed)
        {
            var reports = await _reportRepository.GetByOrderId(request.OrderId).ConfigureAwait(false);
            var reportDetailShopWebResponses = _mapper.Map<List<ReportDetailShopWebResponse>>(reports);
            var order = await _orderRepository.GetIncludeDeliveryPackageById(reportDetailShopWebResponses.Select(r => r.OrderId).First()).ConfigureAwait(false);
            if (order.DeliveryPackage != default && order.DeliveryPackage.ShopDeliveryStaffId != default && order.DeliveryPackage.ShopDeliveryStaffId > 0)
            {
                var staffAccount = _accountRepository.GetById(order.DeliveryPackage.ShopDeliveryStaffId)!;
                reportDetailShopWebResponses.ForEach(report =>
                {
                    report.ShopDeliveryStaffInfo = new ReportDetailShopWebResponse.ShopDeliveryStaffInfoResponse
                    {
                        DeliveryPackageId = order.DeliveryPackageId!.Value,
                        PhoneNumber = staffAccount.PhoneNumber,
                        Email = staffAccount.Email,
                        FullName = staffAccount.FullName,
                        AvatarUrl = staffAccount.AvatarUrl,
                        Id = staffAccount.Id,
                        IsShopOwnerShip = false,
                    };
                });
            }

            var customerInfoResponse = _mapper.Map<ReportDetailShopWebResponse.CustomerInfoResponse>(_accountRepository.GetById(order.CustomerId));

            reportDetailShopWebResponses.ForEach(report =>
            {
                report.CustomerInfo = customerInfoResponse;
            });

            return Result.Success(reportDetailShopWebResponses);
        }
        else
        {
            throw new InvalidBusinessException(MessageCode.E_REPORT_ORDER_NOT_FOUND.GetDescription(), new object[] { request.OrderId });
        }
    }
}