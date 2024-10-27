using MealSync.Application.Common.Repositories;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MealSync.Infrastructure.Persistence.Repositories;

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
    {
    }

    public Task<Order?> GetByIdAndCustomerIdForDetail(long id, long customerId)
    {
        // return DbSet.Include(o => o.OrderDetails)
        //     .ThenInclude(od => od.Food)
        //     .Include(o => o.CustomerLocation)
        //     .Include(o => o.Payments)
        //     .Include(o => o.Shop)
        //     .ThenInclude(s => s.Location)
        //     .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);

        return DbSet.Where(o => o.Id == id && o.CustomerId == customerId)
            .Select(o => new Order
            {
                Id = o.Id,
                FullName = o.FullName,
                PhoneNumber = o.PhoneNumber,
                BuildingName = o.BuildingName,
                Status = o.Status,
                Note = o.Note,
                ShippingFee = o.ShippingFee,
                TotalPrice = o.TotalPrice,
                TotalPromotion = o.TotalPromotion,
                OrderDate = o.OrderDate,
                IntendedReceiveDate = o.IntendedReceiveDate,
                StartTime = o.StartTime,
                EndTime = o.EndTime,
                ReceiveAt = o.ReceiveAt,
                CompletedAt = o.CompletedAt,
                OrderDetails = o.OrderDetails.Select(od => new OrderDetail
                {
                    Id = od.Id,
                    Quantity = od.Quantity,
                    BasicPrice = od.BasicPrice,
                    TotalPrice = od.TotalPrice,
                    Description = od.Description,
                    Food = new Food
                    {
                        Name = od.Food.Name,
                        ImageUrl = od.Food.ImageUrl,
                    },
                }).ToList(),
                Payments = o.Payments.Select(p => new Payment
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Status = p.Status,
                    Type = p.Type,
                    PaymentMethods = p.PaymentMethods,
                }).ToList(),
                Shop = new Shop
                {
                    Id = o.Shop.Id,
                    Name = o.Shop.Name,
                    LogoUrl = o.Shop.LogoUrl,
                },
                ShopLocation = new Location
                {
                    Address = o.ShopLocation.Address,
                    Latitude = o.ShopLocation.Latitude,
                    Longitude = o.ShopLocation.Longitude,
                },
                CustomerLocation = new Location
                {
                    Latitude = o.ShopLocation.Latitude,
                    Longitude = o.ShopLocation.Longitude,
                },
                Promotion = o.Promotion == default
                    ? null
                    : new Promotion
                    {
                        Id = o.Promotion.Id,
                        Title = o.Promotion.Title,
                        BannerUrl = o.Promotion.BannerUrl,
                        Description = o.Promotion.Description,
                        Type = o.Promotion.Type,
                        AmountRate = o.Promotion.AmountRate,
                        MaximumApplyValue = o.Promotion.MaximumApplyValue,
                        AmountValue = o.Promotion.AmountValue,
                        MinOrdervalue = o.Promotion.MinOrdervalue,
                        StartDate = o.Promotion.StartDate,
                        EndDate = o.Promotion.EndDate,
                        ApplyType = o.Promotion.ApplyType,
                    },
                Reviews = o.Reviews.Select(od => new Review
                {
                    Id = od.Id,
                }).ToList(),
            }).FirstOrDefaultAsync();
    }

    public async Task<(int TotalCount, IEnumerable<Order> Orders)> GetByCustomerIdAndStatus(
        long customerId, OrderStatus[]? statusList, bool reviewMode, int pageIndex, int pageSize)
    {
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        var query = DbSet.Where(o => o.CustomerId == customerId);

        if (statusList != default && statusList.Length > 0)
        {
            query = query.Where(o => statusList.Contains(o.Status));
        }
        var projectedQuery = query.Select(o => new Order
            {
                Id = o.Id,
                Status = o.Status,
                ShippingFee = o.ShippingFee,
                TotalPrice = o.TotalPrice,
                TotalPromotion = o.TotalPromotion,
                OrderDate = o.OrderDate,
                IntendedReceiveDate = o.IntendedReceiveDate,
                StartTime = o.StartTime,
                EndTime = o.EndTime,
                Shop = new Shop
                {
                    Name = o.Shop.Name,
                    LogoUrl = o.Shop.LogoUrl,
                },
                OrderDetails = o.OrderDetails.Select(od => new OrderDetail
                {
                    Id = od.Id,
                }).ToList(),
                Reviews = o.Reviews.Select(od => new Review
                {
                    Id = od.Id,
                }).ToList(),
            })
            .ToList();

        var result = projectedQuery.AsEnumerable(); // This brings the data into memory

        if (reviewMode)
        {
            result = result.Where(o =>
                (o.Status == OrderStatus.Delivered ||
                 o.Status == OrderStatus.IssueReported ||
                 o.Status == OrderStatus.UnderReview ||
                 o.Status == OrderStatus.Resolved) &&
                o.Reviews.Count == 0 &&
                now >= new DateTimeOffset(
                    o.IntendedReceiveDate.Year,
                    o.IntendedReceiveDate.Month,
                    o.IntendedReceiveDate.Day,
                    o.EndTime / 100,
                    o.EndTime % 100,
                    0,
                    TimeSpan.FromHours(7)) &&
                now <= new DateTimeOffset(
                    o.IntendedReceiveDate.Year,
                    o.IntendedReceiveDate.Month,
                    o.IntendedReceiveDate.Day,
                    o.EndTime / 100,
                    o.EndTime % 100,
                    0,
                    TimeSpan.FromHours(7)).AddHours(24));
        }

        var totalCount = result.Count();
        var orders = result
            .OrderByDescending(o => o.CreatedDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return (totalCount, orders);
    }

    public Task<Order?> GetByIdAndCustomerIdIncludePayment(long id, long customerId)
    {
        return DbSet.Include(o => o.Payments).FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);
    }

    public Task<bool> CheckExistedByIdAndCustomerId(long id, long customerId)
    {
        return DbSet.AnyAsync(o => o.Id == id && o.CustomerId == customerId);
    }

    public Task<Order?> GetByIdAndCustomerId(long id, long customerId)
    {
        return DbSet.FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);
    }
}