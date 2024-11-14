using MealSync.Application.Common.Repositories;
using MealSync.Application.UseCases.Reports.Models;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class ReportRepository : BaseRepository<Report>, IReportRepository
{
    public ReportRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<bool> CheckExistedCustomerReport(long orderId, long customerId)
    {
        return DbSet.AnyAsync(r => r.OrderId == orderId && r.CustomerId == customerId);
    }

    public Task<bool> CheckExistedShopReplyReport(long orderId, long shopId)
    {
        return DbSet.AnyAsync(r => r.OrderId == orderId && r.ShopId == shopId);
    }

    public Task<long?> GetOrderIdByIdAndCustomerId(long id, long customerId)
    {
        return DbSet
            .Where(r => r.Id == id && r.CustomerId == customerId)
            .Select(r => (long?)r.OrderId)
            .FirstOrDefaultAsync();
    }

    public Task<List<Report>> GetByOrderId(long orderId)
    {
        return DbSet.Where(r => r.OrderId == orderId).ToListAsync();
    }

    public async Task<(int TotalCount, List<ReportByOrderDto> Reports)> GetByShopId(
        long shopId, string? searchValue, ReportStatus? status, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(r => r.Order.ShopId == shopId);

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = double.TryParse(searchValue, out var numericValue);
            string numericString = numericValue.ToString();

            query = query.Where(r =>
                EF.Functions.Like(r.Title, $"%{searchValue}%") ||
                EF.Functions.Like(r.Content, $"%{searchValue}%") ||
                EF.Functions.Like(r.Reason ?? string.Empty, $"%{searchValue}%") ||
                (isNumeric && EF.Functions.Like(r.Id.ToString(), $"%{numericString}%")) ||
                r.Order.OrderDetails.Any(od => EF.Functions.Like(od.Food.Name, $"%{searchValue}%")));
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.CustomerId == default || r.CustomerId == 0 || r.Status == status.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(r => r.CreatedDate >= new DateTimeOffset(dateFrom.Value, TimeSpan.Zero));
        }

        if (dateTo.HasValue)
        {
            query = query.Where(r => r.CreatedDate <= new DateTimeOffset(dateTo.Value, TimeSpan.Zero));
        }

        var groupedQuery = query
            .GroupBy(r => new { r.OrderId, r.Order.IntendedReceiveDate, r.Order.StartTime, r.Order.EndTime })
            .Select(g => new ReportByOrderDto
            {
                MinCreatedDate = g.Min(r => r.CreatedDate),
                OrderId = g.Key.OrderId,
                IntendedReceiveDate = g.Key.IntendedReceiveDate,
                StartTime = g.Key.StartTime,
                EndTime = g.Key.EndTime,
                Description = string.Join(", ", g
                    .Select(r => r.Order)
                    .SelectMany(order => order.OrderDetails
                        .GroupBy(od => new { od.Food.Id, od.Food.Name })
                        .Select(foodGroup => new
                        {
                            Name = foodGroup.Key.Name,
                            Quantity = foodGroup.Sum(od => od.Quantity),
                        }))
                    .Distinct()
                    .Select(fd => fd.Quantity > 1 ? $"{fd.Name} x{fd.Quantity}" : fd.Name)),
                Reports = g.Select(r => new ReportByOrderDto.ReportDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Content = r.Content,
                    ImageUrls = string.IsNullOrEmpty(r.ImageUrl)
                        ? new List<string>()
                        : r.ImageUrl.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                    Status = r.Status,
                    Reason = r.Reason,
                    IsReportedByCustomer = r.CustomerId != default && r.CustomerId > 0,
                    CreatedDate = r.CreatedDate,
                }).ToList(),
            }).OrderByDescending(g => g.MinCreatedDate);

        var reports = await groupedQuery
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync().ConfigureAwait(false);

        var totalCount = await groupedQuery.CountAsync().ConfigureAwait(false);

        return (totalCount, reports);
    }

    public Task<long?> GetOrderIdByIdAndShopId(long id, long shopId)
    {
        return DbSet
            .Where(r => r.Id == id && r.Order.ShopId == shopId && r.CustomerId != default)
            .Select(r => (long?)r.OrderId)
            .FirstOrDefaultAsync();
    }

    public async Task<(int TotalCount, List<Report> Reports)> GetReportForShopWebFilter(
        long shopId, string? searchValue, ReportStatus? status, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(r => r.Order.ShopId == shopId && r.CustomerId.HasValue);

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            searchValue = EscapeLikeParameter(searchValue);
            bool isNumeric = double.TryParse(searchValue, out var numericValue);
            string numericString = numericValue.ToString();

            query = query.Where(r =>
                EF.Functions.Like(r.Title, $"%{searchValue}%") ||
                EF.Functions.Like(r.Content, $"%{searchValue}%") ||
                EF.Functions.Like(r.Reason ?? string.Empty, $"%{searchValue}%") ||
                (isNumeric && EF.Functions.Like(r.Id.ToString(), $"%{numericString}%")) ||
                r.Order.OrderDetails.Any(od => EF.Functions.Like(od.Food.Name, $"%{searchValue}%")));
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.CustomerId == default || r.CustomerId == 0 || r.Status == status.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(r => r.CreatedDate >= new DateTimeOffset(dateFrom.Value, TimeSpan.Zero));
        }

        if (dateTo.HasValue)
        {
            query = query.Where(r => r.CreatedDate <= new DateTimeOffset(dateTo.Value, TimeSpan.Zero));
        }

        query = query.Include(r => r.Order)
            .Include(r => r.Customer)
            .ThenInclude(r => r.Account)
            .Include(r => r.Order)
            .ThenInclude(r => r.DeliveryPackage)
            .ThenInclude(dp => dp.ShopDeliveryStaff)
            .ThenInclude(sds => sds.Account)
            .Include(r => r.Order)
            .ThenInclude(o => o.DeliveryPackage)
            .ThenInclude(dp => dp.Shop)
            .ThenInclude(s => s.Account);

        var reports = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync().ConfigureAwait(false);

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        return (totalCount, reports);
    }

    private static string EscapeLikeParameter(string input)
    {
        return input
            .Replace("\\", "\\\\") // Escape backslash
            .Replace("%", "\\%") // Escape percentage
            .Replace("_", "\\_"); // Escape underscore
    }
}