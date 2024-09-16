using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using MealSync.Domain.Entities;
using MealSync.Domain.Enums;

namespace MealSync.Infrastructure.Persistence.Contexts;

public partial class MealSyncContext : DbContext
{
    public MealSyncContext()
    {
    }

    public MealSyncContext(DbContextOptions<MealSyncContext> options)
        : base(options)
    {
    }

    public async Task<int> SaveChangeAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in this.ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified))
        {
            var now = DateTime.Now;
            entry.Property("UpdatedDate").CurrentValue = now;
            if (entry.State == EntityState.Modified)
            {
                entry.Property("CreatedDate").IsModified = false;
            }

            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedDate").CurrentValue = now;
            }
        }

        var numberChange = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        this.ChangeTracker.Clear();
        return numberChange;
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<VerificationCode> VerificationCodes { get; set; }
    public virtual DbSet<Dormitory> Dormitories { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<Building> Buildings { get; set; }
    public virtual DbSet<CustomerBuilding> CustomerBuildings { get; set; }
    public virtual DbSet<ShopOwner> ShopOwners { get; set; }
    public virtual DbSet<Moderator> Moderators { get; set; }
    public virtual DbSet<ModeratorDormitory> ModeratorDormitories { get; set; }
    public virtual DbSet<Location> Locations { get; set; }
    public virtual DbSet<Wallet> Wallets { get; set; }
    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }
    public virtual DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<AccountPermission> AccountPermissions { get; set; }
    public virtual DbSet<ShopDormitory> ShopDormitories { get; set; }
    public virtual DbSet<WalletHistory> WalletHistories { get; set; }
    public virtual DbSet<OperatingDay> OperatingDays { get; set; }
    public virtual DbSet<OperatingFrame> OperatingFrames { get; set; }
    public virtual DbSet<StaffDelivery> StaffDeliveries { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductOperatingHour> ProductOperatingHours { get; set; }
    public virtual DbSet<ProductQuestion> ProductQuestions { get; set; }
    public virtual DbSet<ProductQuestionOption> ProductQuestionOptions { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<ProductCategory> ProductCategories { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderDetail> OrderDetails { get; set; }
    public virtual DbSet<OrderDetailOption> OrderDetailOptions { get; set; }
    public virtual DbSet<Promotion> Promotions { get; set; }
    public virtual DbSet<OrderTransaction> OrderTransactions { get; set; }
    public virtual DbSet<OrderTransactionHistory> OrderTransactionHistories { get; set; }
    public virtual DbSet<DeliveryOrderCombination> DeliveryOrderCombinations { get; set; }
    public virtual DbSet<Review> Reviews { get; set; }
    public virtual DbSet<Report> Reports { get; set; }
    public virtual DbSet<CommissionConfig> CommissionConfigs { get; set; }
    public virtual DbSet<SystemResource> SystemResources { get; set; }
    public virtual DbSet<ModeratorActivityLog> ModeratorActivityLogs { get; set; }
    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }
    public virtual DbSet<Notification> Notifications { get; set; }
    public virtual DbSet<Favourite> Favourites { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql(Environment.GetEnvironmentVariable("DATABASE_URL"), ServerVersion.Parse("8.0.33-mysql"))
            .UseSnakeCaseNamingConvention();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Account>()
            .HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId)
            .HasConstraintName("FK_Account_Role");
        
        modelBuilder.Entity<VerificationCode>()
            .HasOne(a => a.Account)
            .WithMany(r => r.VerificationCodes)
            .HasForeignKey(a => a.AccountId)
            .HasConstraintName("FK_Account_VerificationCode");
        
        modelBuilder.Entity<Customer>()
            .HasOne(b => b.Account)
            .WithOne(r => r.Customer)
            .HasForeignKey<Customer>(b => b.Id)
            .HasConstraintName("FK_Customer_Account");
        
        modelBuilder.Entity<Customer>()
            .HasOne(b => b.Dormitory)
            .WithMany(d => d.Customers)
            .HasForeignKey(a => a.DormitoryId)
            .HasConstraintName("FK_Customer_Dormitory");
        
        modelBuilder.Entity<Building>()
            .HasOne(b => b.Dormitory)
            .WithMany(d => d.Buildings)
            .HasForeignKey(b => b.DomitoryId)
            .HasConstraintName("FK_Building_Dormitory");
        
        modelBuilder.Entity<Building>()
            .HasOne(b => b.Location)
            .WithOne(l => l.Building)
            .HasForeignKey<Building>(b => b.LocationId)
            .HasConstraintName("FK_Building_Location");

        modelBuilder.Entity<CustomerBuilding>()
            .HasKey(bb => new { bb.BuildingId, bb.CustomerId });
        
        modelBuilder.Entity<CustomerBuilding>()
            .HasOne(bb => bb.Building)
            .WithMany(b => b.CustomerBuildings)
            .HasForeignKey(bb => bb.BuildingId)
            .HasConstraintName("FK_CustomerBuilding_Building");
        
        modelBuilder.Entity<CustomerBuilding>()
            .HasOne(bb => bb.Customer)
            .WithMany(b => b.CustomerBuildings)
            .HasForeignKey(bb => bb.CustomerId)
            .HasConstraintName("FK_CustomerBuilding_Customer");

        modelBuilder.Entity<ShopOwner>()
            .HasOne(so => so.Account)
            .WithOne(a => a.ShopOwner)
            .HasForeignKey<ShopOwner>(so => so.Id)
            .HasConstraintName("FK_ShopOwner_Account");
        
        modelBuilder.Entity<ShopOwner>()
            .HasOne(so => so.Location)
            .WithOne(l => l.ShopOwner)
            .HasForeignKey<ShopOwner>(so => so.LocationId)
            .HasConstraintName("FK_ShopOwner_Location");
        
        modelBuilder.Entity<ShopOwner>()
            .HasOne(so => so.Wallet)
            .WithOne(w => w.ShopOwner)
            .HasForeignKey<ShopOwner>(b => b.WalletId)
            .HasConstraintName("FK_ShopOwner_Wallet");
        
        modelBuilder.Entity<Dormitory>()
            .HasOne(d => d.Location)
            .WithOne(l => l.Dormitory)
            .HasForeignKey<Dormitory>(d => d.LocationId)
            .HasConstraintName("FK_Dormitory_Location");
        
        modelBuilder.Entity<Moderator>()
            .HasOne(so => so.Account)
            .WithOne(a => a.Moderator)
            .HasForeignKey<Moderator>(so => so.Id)
            .HasConstraintName("FK_Moderator_Account");
        
        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.Wallet)
            .WithMany(w => w.WalletTransactions)
            .HasForeignKey(wt => wt.WalletId)
            .HasConstraintName("FK_WalletTransaction_Wallet");
        
        modelBuilder.Entity<WithdrawalRequest>()
            .HasOne(wr => wr.Wallet)
            .WithMany(w => w.WithdrawalRequests)
            .HasForeignKey(wr => wr.WalletId)
            .HasConstraintName("FK_WithdrawalRequest_Wallet");
        
        modelBuilder.Entity<Favourite>()
            .HasKey(f => new { f.CustomerId, f.ShopOwnerId });
        
        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Customer)
            .WithMany(b => b.Favourites)
            .HasForeignKey(f => f.CustomerId)
            .HasConstraintName("FK_Favourite_Customer");
        
        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.ShopOwner)
            .WithMany(b => b.Favourites)
            .HasForeignKey(f => f.ShopOwnerId)
            .HasConstraintName("FK_Favourite_ShopOwner");
        
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Account)
            .WithMany(a => a.Notifications)
            .HasForeignKey(n => n.AccountId)
            .HasConstraintName("FK_Notification_Account");
        
        modelBuilder.Entity<AccountPermission>()
            .HasKey(mp => new { mp.PermissionId, mp.AccountId });
        
        modelBuilder.Entity<AccountPermission>()
            .HasOne(mp => mp.Account)
            .WithMany(a => a.AccountPermissions)
            .HasForeignKey(mp => mp.AccountId)
            .HasConstraintName("FK_AccountPermission_Account");
        
        modelBuilder.Entity<AccountPermission>()
            .HasOne(mp => mp.Permission)
            .WithMany(a => a.AccountPermissions)
            .HasForeignKey(mp => mp.PermissionId)
            .HasConstraintName("FK_AccountPermission_Permission");
        
        modelBuilder.Entity<ShopDormitory>()
            .HasKey(sd => new { sd.ShopOwnerId, sd.DormitoryId });
        
        modelBuilder.Entity<ShopDormitory>()
            .HasOne(sd => sd.ShopOwner)
            .WithMany(s => s.ShopDormitories)
            .HasForeignKey(sd => sd.ShopOwnerId)
            .HasConstraintName("FK_ShopDormitory_ShopOwner");
        
        modelBuilder.Entity<ShopDormitory>()
            .HasOne(sd => sd.Dormitory)
            .WithMany(s => s.ShopDormitories)
            .HasForeignKey(sd => sd.DormitoryId)
            .HasConstraintName("FK_ShopDormitory_Dormitory");
        
        modelBuilder.Entity<OperatingFrame>()
            .HasOne(of => of.OperatingDay)
            .WithMany(od => od.OperatingFrames)
            .HasForeignKey(of => of.OperatingDayId)
            .HasConstraintName("FK_OperatingFrame_OperatingDay");
        
        modelBuilder.Entity<StaffDelivery>()
            .HasOne(sd => sd.ShopOwner)
            .WithMany(so => so.StaffDeliveries)
            .HasForeignKey(sd => sd.ShopOwnerId)
            .HasConstraintName("FK_StaffDelivery_ShopOwner");
        
        modelBuilder.Entity<StaffDelivery>()
            .HasOne(sd => sd.Account)
            .WithOne(a => a.StaffDelivery)
            .HasForeignKey<StaffDelivery>(sd => sd.Id)
            .HasConstraintName("FK_StaffDelivery_Account");
        
        modelBuilder.Entity<Product>()
            .HasOne(p => p.ShopOwner)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.ShopOwnerId)
            .HasConstraintName("FK_Product_ShopOwner");
        
        modelBuilder.Entity<ProductOperatingHour>()
            .HasOne(poh => poh.Product)
            .WithMany(p => p.ProductOperatingHours)
            .HasForeignKey(poh => poh.ProductId)
            .HasConstraintName("FK_ProductOperatingHour_Product");
        
        modelBuilder.Entity<ProductOperatingHour>()
            .HasOne(poh => poh.OperatingDay)
            .WithMany(p => p.ProductOperatingHours)
            .HasForeignKey(poh => poh.OperatingDayId)
            .HasConstraintName("FK_ProductOperatingHour_OperatingDay");
        
        modelBuilder.Entity<ProductQuestion>()
            .HasOne(tq => tq.Product)
            .WithMany(p => p.ProductQuestions)
            .HasForeignKey(tq => tq.ProductId)
            .HasConstraintName("FK_ProductQuestion_Product");
        
        modelBuilder.Entity<ProductQuestionOption>()
            .HasOne(to => to.ProductQuestion)
            .WithMany(tq => tq.ProductQuestionOptions)
            .HasForeignKey(to => to.ToppingQuestionId)
            .HasConstraintName("FK_ToppingOption_ToppingQuestion");

        modelBuilder.Entity<ProductCategory>()
            .HasKey(pc => new { pc.CategoryId, pc.ProductId });

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId)
            .HasConstraintName("FK_ProductCategory_Product");
        
        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId)
            .HasConstraintName("FK_ProductCategory_Category");
        
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Product)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.ProductId)
            .HasConstraintName("FK_OrderDetail_Product");
        
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .HasConstraintName("FK_OrderDetail_Order");

        modelBuilder.Entity<OrderDetailOption>()
            .HasKey(odo => new { odo.OrderDetailId, odo.ToppingOptionId });
        
        modelBuilder.Entity<OrderDetailOption>()
            .HasOne(odo => odo.OrderDetail)
            .WithMany(od => od.OrderDetailOptions)
            .HasForeignKey(odo => odo.OrderDetailId)
            .HasConstraintName("FK_OrderDetailOption_OrderDetail");
        
        modelBuilder.Entity<OrderDetailOption>()
            .HasOne(odo => odo.ProductQuestionOption)
            .WithMany(tp => tp.OrderDetailOptions)
            .HasForeignKey(odo => odo.ToppingOptionId)
            .HasConstraintName("FK_OrderDetailOption_ToppingOption");
        
        modelBuilder.Entity<Promotion>()
            .HasOne(p => p.Customer)
            .WithMany(tp => tp.Promotions)
            .HasForeignKey(p => p.CustomerId)
            .HasConstraintName("FK_Promotion_Customer");
        
        modelBuilder.Entity<Promotion>()
            .HasOne(p => p.ShopOwner)
            .WithMany(s => s.Promotions)
            .HasForeignKey(p => p.ShopOwnerId)
            .HasConstraintName("FK_Promotion_ShopOwner");
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Promotion)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.PromotionId)
            .HasConstraintName("FK_Order_Promotion");
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.ShopOwner)
            .WithMany(so => so.Orders)
            .HasForeignKey(o => o.ShopOwnerId)
            .HasConstraintName("FK_Order_ShopOwner");
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.CustomerId)
            .HasConstraintName("FK_Order_Customer");
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.DeliveryOrderCombination)
            .WithMany(doc => doc.Orders)
            .HasForeignKey(o => o.DeliveryOrderCombinationId)
            .HasConstraintName("FK_Order_DeliveryOrderCombination");
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.ShopLocation)
            .WithOne(l => l.OrderShop)
            .HasForeignKey<Order>(o => o.ShopLocationId)
            .HasConstraintName("FK_Order_ShopLocation");
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.CustomerLocation)
            .WithOne(l => l.OrderCustomer)
            .HasForeignKey<Order>(o => o.CustomerLocationId)
            .HasConstraintName("FK_Order_CustomerLocation");
        
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Building)
            .WithMany(l => l.Orders)
            .HasForeignKey(o => o.BuildingId)
            .HasConstraintName("FK_Order_Building");
        
        modelBuilder.Entity<OrderTransaction>()
            .HasOne(ot => ot.Order)
            .WithMany(o => o.OrderTransactions)
            .HasForeignKey(ot => ot.OrderId)
            .HasConstraintName("FK_OrderTransaction_Order");
        
        modelBuilder.Entity<DeliveryOrderCombination>()
            .HasOne(doc => doc.StaffDelivery)
            .WithMany(sd => sd.DeliveryOrderCombinations)
            .HasForeignKey(doc => doc.StaffDeliveryId)
            .HasConstraintName("FK_DeliveryOrderCombination_StaffDelivery");
        
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.CustomerId)
            .HasConstraintName("FK_Review_Customer");
        
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Order)
            .WithMany(o => o.Reviews)
            .HasForeignKey(r => r.OrderId)
            .HasConstraintName("FK_Review_Order");
        
        modelBuilder.Entity<Report>()
            .HasOne(r => r.ShopOwner)
            .WithMany(so => so.Reports)
            .HasForeignKey(r => r.ShopOwnerId)
            .HasConstraintName("FK_Report_ShopOwner");
        
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Customer)
            .WithMany(b => b.Reports)
            .HasForeignKey(r => r.CustomerId)
            .HasConstraintName("FK_Report_Customer");
        
        modelBuilder.Entity<Report>()
            .HasOne(r => r.StaffDelivery)
            .WithMany(sd => sd.Reports)
            .HasForeignKey(r => r.StaffDeliveryId)
            .HasConstraintName("FK_Report_StaffDelivery");
        
        modelBuilder.Entity<Report>()
            .HasOne(r => r.Order)
            .WithMany(o => o.Reports)
            .HasForeignKey(r => r.OrderId)
            .HasConstraintName("FK_Report_Order");
        
        modelBuilder.Entity<ModeratorActivityLog>()
            .HasOne(mal => mal.Moderator)
            .WithMany(a => a.ModeratorActivityLogs)
            .HasForeignKey(mal => mal.ModeratorId)
            .HasConstraintName("FK_ModeratorActivityLog_Moderator");

        modelBuilder.Entity<ModeratorDormitory>()
            .HasKey(md => new { md.ModeratorId, md.DormitoryId });
            
        modelBuilder.Entity<ModeratorDormitory>()
            .HasOne(md => md.Moderator)
            .WithMany(m => m.ModeratorDormitories)
            .HasForeignKey(mal => mal.ModeratorId)
            .HasConstraintName("FK_ModeratorDormitory_Moderator");
        
        modelBuilder.Entity<ModeratorDormitory>()
            .HasOne(md => md.Dormitory)
            .WithMany(m => m.ModeratorDormitories)
            .HasForeignKey(mal => mal.DormitoryId)
            .HasConstraintName("FK_ModeratorDormitory_Dormitory");
    }
}
