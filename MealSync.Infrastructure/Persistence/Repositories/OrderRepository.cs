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

    public Task<Order?> GetByIdAndCustomerId(long id, long customerId)
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
            }).FirstOrDefaultAsync();
    }

    public async Task<(int TotalCount, IEnumerable<Order> Orders)> GetByCustomerIdAndStatus(
        long customerId, List<OrderStatus>? statusList, int pageIndex, int pageSize)
    {
        var query = DbSet.Where(o => o.CustomerId == customerId);

        if (statusList != default && statusList.Count > 0)
        {
            query = query.Where(o => statusList.Contains(o.Status));
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);
        var orders = await query
            .OrderByDescending(o => o.CreatedDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new Order
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
            })
            .ToListAsync().ConfigureAwait(false);

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
}