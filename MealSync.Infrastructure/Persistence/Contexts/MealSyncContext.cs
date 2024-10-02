using Microsoft.EntityFrameworkCore;
using MealSync.Domain.Entities;
using MealSync.Infrastructure.Persistence.Interceptors;

namespace MealSync.Infrastructure.Persistence.Contexts;

public partial class MealSyncContext : DbContext
{
    private readonly CustomSaveChangesInterceptor _customSaveChangesInterceptor;

    public MealSyncContext(DbContextOptions<MealSyncContext> options, CustomSaveChangesInterceptor customSaveChangesInterceptor)
        : base(options)
    {
        _customSaveChangesInterceptor = customSaveChangesInterceptor;
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Dormitory> Dormitories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<CustomerBuilding> CustomerBuildings { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<Moderator> Moderators { get; set; }

    public virtual DbSet<ModeratorDormitory> ModeratorDormitories { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    public virtual DbSet<WalletTransaction> WalletTransactions { get; set; }

    public virtual DbSet<WithdrawalRequest> WithdrawalRequests { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<AccountPermission> AccountPermissions { get; set; }

    public virtual DbSet<ShopDormitory> ShopDormitories { get; set; }

    public virtual DbSet<OperatingSlot> OperatingSlots { get; set; }

    public virtual DbSet<StaffDelivery> StaffDeliveries { get; set; }

    public virtual DbSet<Food> Foods { get; set; }

    public virtual DbSet<OptionGroup> OptionGroups { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<FoodOptionGroup> FoodOptionGroups { get; set; }

    public virtual DbSet<FoodOperatingSlot> FoodOperatingSlots { get; set; }

    public virtual DbSet<PlatformCategory> PlatformCategories { get; set; }

    public virtual DbSet<ShopCategory> ShopCategories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<OrderDetailOption> OrderDetailOptions { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentHistory> PaymentHistories { get; set; }

    public virtual DbSet<DeliveryPackage> DeliveryPackages { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Report> Reports { get; set; }

    public virtual DbSet<CommissionConfig> CommissionConfigs { get; set; }

    public virtual DbSet<SystemResource> SystemResources { get; set; }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Favourite> Favourites { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_customSaveChangesInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Role)
            .WithMany(r => r.Accounts)
            .HasForeignKey(a => a.RoleId)
            .HasConstraintName("FK_Account_Role");

        modelBuilder.Entity<Customer>()
            .HasOne(b => b.Account)
            .WithOne(r => r.Customer)
            .HasForeignKey<Customer>(b => b.Id)
            .HasConstraintName("FK_Customer_Account");

        modelBuilder.Entity<Building>()
            .HasOne(b => b.Dormitory)
            .WithMany(d => d.Buildings)
            .HasForeignKey(b => b.DormitoryId)
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

        modelBuilder.Entity<Shop>()
            .HasOne(so => so.Account)
            .WithOne(a => a.Shop)
            .HasForeignKey<Shop>(so => so.Id)
            .HasConstraintName("FK_Shop_Account");

        modelBuilder.Entity<Shop>()
            .HasOne(so => so.Location)
            .WithOne(l => l.Shop)
            .HasForeignKey<Shop>(so => so.LocationId)
            .HasConstraintName("FK_Shop_Location");

        modelBuilder.Entity<Shop>()
            .HasOne(so => so.Wallet)
            .WithOne(w => w.Shop)
            .HasForeignKey<Shop>(b => b.WalletId)
            .HasConstraintName("FK_Shop_Wallet");

        modelBuilder.Entity<ShopCategory>()
            .HasOne(sc => sc.Shop)
            .WithMany(w => w.ShopCategories)
            .HasForeignKey(sc => sc.ShopId)
            .HasConstraintName("FK_ShopCategory_Shop");

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
            .HasOne(wt => wt.WalletFrom)
            .WithMany(w => w.WalletTransactionFroms)
            .HasForeignKey(wt => wt.WalletFromId)
            .HasConstraintName("FK_WalletTransaction_WalletFrom");

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.WalletTo)
            .WithMany(w => w.WalletTransactionTos)
            .HasForeignKey(wt => wt.WalletToId)
            .HasConstraintName("FK_WalletTransaction_WalletTo");

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.WithdrawalRequest)
            .WithOne(wr => wr.WalletTransaction)
            .HasForeignKey<WalletTransaction>(wt => wt.WithdrawalRequestId)
            .HasConstraintName("FK_WalletTransaction_WithdrawalRequest");

        modelBuilder.Entity<WalletTransaction>()
            .HasOne(wt => wt.Payment)
            .WithOne(p => p.WalletTransaction)
            .HasForeignKey<WalletTransaction>(wt => wt.PaymentId)
            .HasConstraintName("FK_WalletTransaction_Payment");

        modelBuilder.Entity<WithdrawalRequest>()
            .HasOne(wr => wr.Wallet)
            .WithMany(w => w.WithdrawalRequests)
            .HasForeignKey(wr => wr.WalletId)
            .HasConstraintName("FK_WithdrawalRequest_Wallet");

        modelBuilder.Entity<Favourite>()
            .HasKey(f => new { f.CustomerId, f.ShopId });

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Customer)
            .WithMany(b => b.Favourites)
            .HasForeignKey(f => f.CustomerId)
            .HasConstraintName("FK_Favourite_Customer");

        modelBuilder.Entity<Favourite>()
            .HasOne(f => f.Shop)
            .WithMany(b => b.Favourites)
            .HasForeignKey(f => f.ShopId)
            .HasConstraintName("FK_Favourite_Shop");

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
            .HasKey(sd => new { sd.ShopId, sd.DormitoryId });

        modelBuilder.Entity<ShopDormitory>()
            .HasOne(sd => sd.Shop)
            .WithMany(s => s.ShopDormitories)
            .HasForeignKey(sd => sd.ShopId)
            .HasConstraintName("FK_ShopDormitory_Shop");

        modelBuilder.Entity<ShopDormitory>()
            .HasOne(sd => sd.Dormitory)
            .WithMany(s => s.ShopDormitories)
            .HasForeignKey(sd => sd.DormitoryId)
            .HasConstraintName("FK_ShopDormitory_Dormitory");

        modelBuilder.Entity<StaffDelivery>()
            .HasOne(sd => sd.Shop)
            .WithMany(so => so.StaffDeliveries)
            .HasForeignKey(sd => sd.ShopId)
            .HasConstraintName("FK_StaffDelivery_Shop");

        modelBuilder.Entity<StaffDelivery>()
            .HasOne(sd => sd.Account)
            .WithOne(a => a.StaffDelivery)
            .HasForeignKey<StaffDelivery>(sd => sd.Id)
            .HasConstraintName("FK_StaffDelivery_Account");

        modelBuilder.Entity<Food>()
            .HasOne(p => p.PlatformCategory)
            .WithMany(c => c.Foods)
            .HasForeignKey(p => p.PlatformCategoryId)
            .HasConstraintName("FK_Food_PlatformCategory");

        modelBuilder.Entity<Food>()
            .HasOne(p => p.Shop)
            .WithMany(s => s.Foods)
            .HasForeignKey(p => p.ShopId)
            .HasConstraintName("FK_Food_Shop");

        modelBuilder.Entity<Food>()
            .HasOne(p => p.ShopCategory)
            .WithMany(sc => sc.Foods)
            .HasForeignKey(p => p.ShopCategoryId)
            .HasConstraintName("FK_Food_ShopCategory");

        modelBuilder.Entity<OptionGroup>()
            .HasOne(og => og.Shop)
            .WithMany(p => p.OptionGroups)
            .HasForeignKey(og => og.ShopId)
            .HasConstraintName("FK_OptionGroup_Shop");

        modelBuilder.Entity<Option>()
            .HasOne(to => to.OptionGroup)
            .WithMany(tq => tq.Options)
            .HasForeignKey(to => to.OptionGroupId)
            .HasConstraintName("FK_Option_OptionGroup");

        modelBuilder.Entity<FoodOptionGroup>()
            .HasKey(fog => new
            {
                fog.FoodId,
                fog.OptionGroupId,
            });

        modelBuilder.Entity<FoodOptionGroup>()
            .HasOne(fog => fog.Food)
            .WithMany(s => s.FoodOptionGroups)
            .HasForeignKey(fog => fog.FoodId)
            .HasConstraintName("FK_FoodOptionGroup_Food");

        modelBuilder.Entity<FoodOptionGroup>()
            .HasOne(fog => fog.OptionGroup)
            .WithMany(s => s.FoodOptionGroups)
            .HasForeignKey(fog => fog.OptionGroupId)
            .HasConstraintName("FK_FoodOptionGroup_OptionGroup");

        modelBuilder.Entity<FoodOperatingSlot>()
            .HasKey(pc => new { pc.OperatingSlotId, pc.FoodId });

        modelBuilder.Entity<FoodOperatingSlot>()
            .HasOne(pos => pos.Food)
            .WithMany(p => p.FoodOperatingSlots)
            .HasForeignKey(pc => pc.FoodId)
            .HasConstraintName("FK_FoodOperatingSlot_Food");

        modelBuilder.Entity<FoodOperatingSlot>()
            .HasOne(pos => pos.OperatingSlot)
            .WithMany(os => os.FoodOperatingSlots)
            .HasForeignKey(pc => pc.OperatingSlotId)
            .HasConstraintName("FK_FoodOperatingSlot_OperatingSlot");

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Food)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.FoodId)
            .HasConstraintName("FK_OrderDetail_Food");

        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .HasConstraintName("FK_OrderDetail_Order");

        modelBuilder.Entity<OrderDetailOption>()
            .HasKey(odo => new
            {
                odo.OrderDetailId,
                odo.OptionId,
            });

        modelBuilder.Entity<OrderDetailOption>()
            .HasOne(odo => odo.OrderDetail)
            .WithMany(od => od.OrderDetailOptions)
            .HasForeignKey(odo => odo.OrderDetailId)
            .HasConstraintName("FK_OrderDetailOption_OrderDetail");

        modelBuilder.Entity<OrderDetailOption>()
            .HasOne(odo => odo.Option)
            .WithMany(tp => tp.OrderDetailOptions)
            .HasForeignKey(odo => odo.OptionId)
            .HasConstraintName("FK_OrderDetailOption_Option");

        modelBuilder.Entity<Promotion>()
            .HasOne(p => p.Customer)
            .WithMany(tp => tp.Promotions)
            .HasForeignKey(p => p.CustomerId)
            .HasConstraintName("FK_Promotion_Customer");

        modelBuilder.Entity<Promotion>()
            .HasOne(p => p.Shop)
            .WithMany(s => s.Promotions)
            .HasForeignKey(p => p.ShopId)
            .HasConstraintName("FK_Promotion_Shop");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Promotion)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.PromotionId)
            .HasConstraintName("FK_Order_Promotion");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Shop)
            .WithMany(so => so.Orders)
            .HasForeignKey(o => o.ShopId)
            .HasConstraintName("FK_Order_Shop");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(b => b.Orders)
            .HasForeignKey(o => o.CustomerId)
            .HasConstraintName("FK_Order_Customer");

        modelBuilder.Entity<Order>()
            .HasOne(o => o.DeliveryPackage)
            .WithMany(doc => doc.Orders)
            .HasForeignKey(o => o.DeliveryPackageId)
            .HasConstraintName("FK_Order_DeliveryPackage");

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

        modelBuilder.Entity<Payment>()
            .HasOne(ot => ot.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(ot => ot.OrderId)
            .HasConstraintName("FK_Payment_Order");

        modelBuilder.Entity<DeliveryPackage>()
            .HasOne(doc => doc.StaffDelivery)
            .WithMany(sd => sd.DeliveryPackages)
            .HasForeignKey(doc => doc.StaffDeliveryId)
            .HasConstraintName("FK_DeliveryPackage_StaffDelivery");

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
            .HasOne(r => r.Shop)
            .WithMany(so => so.Reports)
            .HasForeignKey(r => r.ShopId)
            .HasConstraintName("FK_Report_Shop");

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

        modelBuilder.Entity<ActivityLog>()
            .HasOne(mal => mal.Account)
            .WithMany(a => a.ActivityLogs)
            .HasForeignKey(mal => mal.AccountId)
            .HasConstraintName("FK_ActivityLog_Account");

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

        modelBuilder.Entity<OperatingSlot>()
            .HasOne(od => od.Shop)
            .WithMany(so => so.OperatingSlots)
            .HasForeignKey(od => od.ShopId)
            .HasConstraintName("FK_OperatingSlot_Shop");
    }
}