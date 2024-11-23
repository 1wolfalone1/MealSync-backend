﻿using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Application.Common.Repositories;

public interface IReportRepository : IBaseRepository<Report>
{
    Task<bool> CheckExistedCustomerReport(long orderId, long customerId);

    Task<bool> CheckExistedShopReplyReport(long orderId, long shopId);

    Task<long?> GetOrderIdByIdAndCustomerId(long id, long customerId);

    Task<List<Report>> GetByOrderId(long orderId);

    Task<(int TotalCount, List<ReportByOrderDto> Reports)> GetByShopId(
        long shopId, string? searchValue, ReportStatus? status, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize);

    Task<long?> GetOrderIdByIdAndShopId(long id, long shopId);

    Task<(int TotalCount, List<Report> Reports)> GetReportForShopWebFilter(
        long shopId, string? searchValue, ReportStatus? status, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize);

    Task<bool> CheckExistedByOrderIdAndShopId(long orderId, long shopId);
}