using AutoMapper;
using MealSync.Application.Common.Abstractions.Messaging;
using MealSync.Application.Common.Enums;
using MealSync.Application.Common.Repositories;
using MealSync.Application.Common.Services;
using MealSync.Application.Shared;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Enums;
using MealSync.Domain.Exceptions.Base;

namespace MealSync.Application.UseCases.Reports.Queries.GetReportDetailForMod;

public class GetReportDetailForModHandler : IQueryHandler<GetReportDetailForModQuery, Result>
{
    private readonly ICurrentPrincipalService _currentPrincipalService;
    private readonly IModeratorDormitoryRepository _moderatorDormitoryRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IShopRepository _shopRepository;
    private readonly IMapper _mapper;

    public GetReportDetailForModHandler(
        ICurrentPrincipalService currentPrincipalService, IModeratorDormitoryRepository moderatorDormitoryRepository,
        IReportRepository reportRepository, IAccountRepository accountRepository,
        IOrderRepository orderRepository, IShopRepository shopRepository, IMapper mapper)
    {
        _currentPrincipalService = currentPrincipalService;
        _moderatorDormitoryRepository = moderatorDormitoryRepository;
        _reportRepository = reportRepository;
        _accountRepository = accountRepository;
        _orderRepository = orderRepository;
        _shopRepository = shopRepository;
        _mapper = mapper;
    }

    public async Task<Result<Result>> Handle(GetReportDetailForModQuery request, CancellationToken cancellationToken)
    {
        var moderatorAccountId = _currentPrincipalService.CurrentPrincipalId!.Value;
        var dormitories = await _moderatorDormitoryRepository.GetAllDormitoryByModeratorId(moderatorAccountId).ConfigureAwait(false);
        var dormitoryIds = dormitories.Select(d => d.DormitoryId).ToList();

        var orderId = await _reportRepository.GetOrderIdByReportIdAndDormitoryIds(request.ReportId, dormitoryIds).ConfigureAwait(false);

        if (orderId == default || orderId == 0)
        {
            throw new InvalidBusinessException(MessageCode.E_MODERATOR_ACTION_NOT_ALLOW.GetDescription());
        }
        else
        {
            var reports = await _reportRepository.GetByOrderId(orderId.Value).ConfigureAwait(false);
            var reportDetailShopWebResponses = _mapper.Map<List<ReportDetailForModResponse>>(reports);
            var order = await _orderRepository.GetIncludeDeliveryPackageById(reportDetailShopWebResponses.Select(r => r.OrderId).First()).ConfigureAwait(false);
            if (order.DeliveryPackage != default && order.DeliveryPackage.ShopDeliveryStaffId != default && order.DeliveryPackage.ShopDeliveryStaffId > 0)
            {
                var staffAccount = _accountRepository.GetById(order.DeliveryPackage.ShopDeliveryStaffId)!;
                reportDetailShopWebResponses.ForEach(report =>
                {
                    report.ShopDeliveryStaffInfo = new ReportDetailForModResponse.ShopDeliveryStaffInfoForModResponse()
                    {
                        DeliveryPackageId = order.DeliveryPackageId!.Value,
                        PhoneNumber = staffAccount.PhoneNumber,
                        Email = staffAccount.Email,
                        FullName = staffAccount.FullName,
                        AvatarUrl = staffAccount.AvatarUrl,
                        Id = staffAccount.Id,
                    };
                });
            }

            var customerInfoResponse = _mapper.Map<ReportDetailForModResponse.CustomerInfoForModResponse>(_accountRepository.GetIncludeCustomerById(order.CustomerId));
            var shopInfoResponse = _mapper.Map<ReportDetailForModResponse.ShopInfoForModResponse>(_shopRepository.GetById(order.ShopId));

            reportDetailShopWebResponses.ForEach(report =>
            {
                report.CustomerInfo = customerInfoResponse;
                report.ShopInfo = shopInfoResponse;
            });

            return Result.Success(reportDetailShopWebResponses);
        }
    }
}