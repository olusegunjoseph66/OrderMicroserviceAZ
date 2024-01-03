using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Shared.Data.Models
{
    public partial class dmsdevdbContext : DbContext
    {
        public dmsdevdbContext()
        {
        }

        public dmsdevdbContext(DbContextOptions<dmsdevdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<DeliveryBlock> DeliveryBlocks { get; set; } = null!;
        public virtual DbSet<DeliveryMethod> DeliveryMethods { get; set; } = null!;
        public virtual DbSet<DeliveryStatus> DeliveryStatuses { get; set; } = null!;
        public virtual DbSet<DistributorSapAccount> DistributorSapAccounts { get; set; } = null!;
        public virtual DbSet<DmsOrder> DmsOrders { get; set; } = null!;
        public virtual DbSet<DmsOrderGroup> DmsOrderGroups { get; set; } = null!;
        public virtual DbSet<DmsOrderItem> DmsOrderItems { get; set; } = null!;
        public virtual DbSet<DmsOrdersChangeLog> DmsOrdersChangeLogs { get; set; } = null!;
        public virtual DbSet<OrderStatus> OrderStatuses { get; set; } = null!;
        public virtual DbSet<OrderType> OrderTypes { get; set; } = null!;
        public virtual DbSet<Otp> Otps { get; set; } = null!;
        public virtual DbSet<OtpStatus> OtpStatuses { get; set; } = null!;
        public virtual DbSet<Plant> Plants { get; set; } = null!;
        public virtual DbSet<PlantType> PlantTypes { get; set; } = null!;
        public virtual DbSet<PlantsBk> PlantsBks { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; } = null!;
        public virtual DbSet<ShoppingCartItem> ShoppingCartItems { get; set; } = null!;
        public virtual DbSet<ShoppingCartStatus> ShoppingCartStatuses { get; set; } = null!;
        public virtual DbSet<TripStatus> TripStatuses { get; set; } = null!;
        public virtual DbSet<TruckSize> TruckSizes { get; set; } = null!;
        public virtual DbSet<TruckSizeDeliveryMethodMapping> TruckSizeDeliveryMethodMappings { get; set; } = null!;
        public virtual DbSet<UnitOfMeasure> UnitOfMeasures { get; set; } = null!;

      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeliveryBlock>(entity =>
            {
                entity.ToTable("DeliveryBlocks", "Orders");

                entity.HasIndex(e => e.Code, "IX_DeliveryBlocks")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_DeliveryBlocks_1")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DeliveryMethod>(entity =>
            {
                entity.ToTable("DeliveryMethods", "Orders");

                entity.HasIndex(e => e.Code, "IX_DeliveryMethods")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_DeliveryMethods_1")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DeliveryStatus>(entity =>
            {
                entity.ToTable("DeliveryStatuses", "Orders");

                entity.HasIndex(e => e.Code, "IX_DeliveryStatuses")
                    .IsUnique();

                entity.HasIndex(e => e.Code, "IX_DeliveryStatuses_1")
                    .IsUnique();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DistributorSapAccount>(entity =>
            {
                entity.ToTable("DistributorSapAccounts", "Orders");

                entity.Property(e => e.DistributorSapAccountId).ValueGeneratedNever();

                entity.Property(e => e.AccountType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DateRefreshed).HasColumnType("datetime");

                entity.Property(e => e.DistributorName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.DistributorSapNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DmsOrder>(entity =>
            {
                entity.ToTable("DmsOrders", "Orders");

                entity.Property(e => e.ChannelCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerPaymentDate).HasColumnType("datetime");

                entity.Property(e => e.CustomerPaymentReference)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.DateRefreshed).HasColumnType("datetime");

                entity.Property(e => e.DateSubmittedOnDms)
                    .HasColumnType("datetime")
                    .HasColumnName("DateSubmittedOnDMS");

                entity.Property(e => e.DateSubmittedToSap)
                    .HasColumnType("datetime")
                    .HasColumnName("DateSubmittedToSAP");

                entity.Property(e => e.DeliveryAddress).IsUnicode(false);

                entity.Property(e => e.DeliveryBlockCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryCity)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryCountryCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryDate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryMethodCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DeliverySapNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryStateCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryStatusCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EstimatedNetValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.IsAtc).HasColumnName("IsATC");

                entity.Property(e => e.NumberOfChildAtc).HasColumnName("NumberOfChildATC");

                entity.Property(e => e.NumberOfSapsubmissionAttempts).HasColumnName("NumberOfSAPSubmissionAttempts");

                entity.Property(e => e.OrderSapNetValue).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.OrderSapNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ParentOrderSapNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.PlantCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.SapDateCreated).HasColumnType("datetime");

                entity.Property(e => e.SapFreightCharges).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SapReference).HasMaxLength(50);

                entity.Property(e => e.SapTripNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SapVat).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.TripDispatchDate).HasColumnType("datetime");

                entity.Property(e => e.TripStatusCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TruckSizeCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ValidFrom).HasColumnType("datetime");

                entity.Property(e => e.ValidTo).HasColumnType("datetime");

                entity.Property(e => e.WayBillNumber)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.DeliveryStatus)
                    .WithMany(p => p.DmsOrders)
                    .HasForeignKey(d => d.DeliveryStatusId)
                    .HasConstraintName("FK_DmsOrders_DeliveryStatus");

                entity.HasOne(d => d.DistributorSapAccount)
                    .WithMany(p => p.DmsOrders)
                    .HasForeignKey(d => d.DistributorSapAccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DmsOrders_DistributorSapAccounts");

                entity.HasOne(d => d.DmsOrderGroup)
                    .WithMany(p => p.DmsOrders)
                    .HasForeignKey(d => d.DmsOrderGroupId)
                    .HasConstraintName("FK_DmsOrders_DmsOrderGroup");

                entity.HasOne(d => d.OrderStatus)
                    .WithMany(p => p.DmsOrders)
                    .HasForeignKey(d => d.OrderStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DmsOrders_OrderStatuses");

                entity.HasOne(d => d.OrderType)
                    .WithMany(p => p.DmsOrders)
                    .HasForeignKey(d => d.OrderTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DmsOrders_OrderTypes");

                entity.HasOne(d => d.Plant)
                    .WithMany(p => p.DmsOrders)
                    .HasForeignKey(d => d.PlantId)
                    .HasConstraintName("FK_DmsOrders_Plants");

                entity.HasOne(d => d.ShoppingCart)
                    .WithMany(p => p.DmsOrders)
                    .HasForeignKey(d => d.ShoppingCartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DmsOrders_ShoppingCarts");
            });

            modelBuilder.Entity<DmsOrderGroup>(entity =>
            {
                entity.ToTable("DmsOrderGroup", "Orders");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.HasOne(d => d.ShoppingCart)
                    .WithMany(p => p.DmsOrderGroups)
                    .HasForeignKey(d => d.ShoppingCartId)
                    .HasConstraintName("FK_DmsOrderGroup_ShoppingCarts");
            });

            modelBuilder.Entity<DmsOrderItem>(entity =>
            {
                entity.ToTable("DmsOrderItems", "Orders");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.OrderItemSapNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SalesUnitOfMeasureCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SapDeliveryQuality).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.SapNetValue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.SapPricePerUnit).HasColumnType("decimal(18, 0)");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.DmsOrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DmsOrderItems_DmsOrders");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.DmsOrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DmsOrderItems_Products");
            });

            modelBuilder.Entity<DmsOrdersChangeLog>(entity =>
            {
                entity.ToTable("DmsOrders_ChangeLogs", "Orders");

                entity.Property(e => e.ChangeType)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.NewChannelCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("new_ChannelCode");

                entity.Property(e => e.NewCompanyCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("new_CompanyCode");

                entity.Property(e => e.NewCountryCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("new_CountryCode");

                entity.Property(e => e.NewCreatedByUserId).HasColumnName("new_CreatedByUserId");

                entity.Property(e => e.NewDateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("new_DateCreated");

                entity.Property(e => e.NewDateModified)
                    .HasColumnType("datetime")
                    .HasColumnName("new_DateModified");

                entity.Property(e => e.NewDateRefreshed)
                    .HasColumnType("datetime")
                    .HasColumnName("new_DateRefreshed");

                entity.Property(e => e.NewDateSubmittedOnDms)
                    .HasColumnType("datetime")
                    .HasColumnName("new_DateSubmittedOnDMS");

                entity.Property(e => e.NewDateSubmittedToSap)
                    .HasColumnType("datetime")
                    .HasColumnName("new_DateSubmittedToSAP");

                entity.Property(e => e.NewDeliveryAddress)
                    .IsUnicode(false)
                    .HasColumnName("new_DeliveryAddress");

                entity.Property(e => e.NewDeliveryCity)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("new_DeliveryCity");

                entity.Property(e => e.NewDeliveryCountryCode)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("new_DeliveryCountryCode");

                entity.Property(e => e.NewDeliveryDate)
                    .HasColumnType("datetime")
                    .HasColumnName("new_DeliveryDate");

                entity.Property(e => e.NewDeliveryMethodCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("new_DeliveryMethodCode");

                entity.Property(e => e.NewDeliveryStateCode)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("new_DeliveryStateCode");

                entity.Property(e => e.NewDistributorSapAccountId).HasColumnName("new_DistributorSapAccountId");

                entity.Property(e => e.NewEstimatedNetValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("new_EstimatedNetValue");

                entity.Property(e => e.NewModifiedByUserId).HasColumnName("new_ModifiedByUserId");

                entity.Property(e => e.NewNumberOfSapsubmissionAttempts).HasColumnName("new_NumberOfSAPSubmissionAttempts");

                entity.Property(e => e.NewOrderSapNetValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("new_OrderSapNetValue");

                entity.Property(e => e.NewOrderSapNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("new_OrderSapNumber");

                entity.Property(e => e.NewOrderStatusId).HasColumnName("new_OrderStatusId");

                entity.Property(e => e.NewOrderTypeId).HasColumnName("new_OrderTypeId");

                entity.Property(e => e.NewPlantCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("new_PlantCode");

                entity.Property(e => e.NewPlantId).HasColumnName("new_PlantId");

                entity.Property(e => e.NewShoppingCartId).HasColumnName("new_ShoppingCartId");

                entity.Property(e => e.NewTruckSizeCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("new_TruckSizeCode");

                entity.Property(e => e.NewUserId).HasColumnName("new_UserId");

                entity.Property(e => e.OldChannelCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Old_ChannelCode");

                entity.Property(e => e.OldCompanyCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Old_CompanyCode");

                entity.Property(e => e.OldCountryCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Old_CountryCode");

                entity.Property(e => e.OldCreatedByUserId).HasColumnName("Old_CreatedByUserId");

                entity.Property(e => e.OldDateCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("Old_DateCreated");

                entity.Property(e => e.OldDateModified)
                    .HasColumnType("datetime")
                    .HasColumnName("Old_DateModified");

                entity.Property(e => e.OldDateRefreshed)
                    .HasColumnType("datetime")
                    .HasColumnName("Old_DateRefreshed");

                entity.Property(e => e.OldDateSubmittedOnDms)
                    .HasColumnType("datetime")
                    .HasColumnName("Old_DateSubmittedOnDMS");

                entity.Property(e => e.OldDateSubmittedToSap)
                    .HasColumnType("datetime")
                    .HasColumnName("Old_DateSubmittedToSAP");

                entity.Property(e => e.OldDeliveryAddress)
                    .IsUnicode(false)
                    .HasColumnName("Old_DeliveryAddress");

                entity.Property(e => e.OldDeliveryCity)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Old_DeliveryCity");

                entity.Property(e => e.OldDeliveryCountryCode)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("Old_DeliveryCountryCode");

                entity.Property(e => e.OldDeliveryDate)
                    .HasColumnType("datetime")
                    .HasColumnName("Old_DeliveryDate");

                entity.Property(e => e.OldDeliveryMethodCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Old_DeliveryMethodCode");

                entity.Property(e => e.OldDeliveryStateCode)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("Old_DeliveryStateCode");

                entity.Property(e => e.OldDistributorSapAccountId).HasColumnName("Old_DistributorSapAccountId");

                entity.Property(e => e.OldEstimatedNetValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("Old_EstimatedNetValue");

                entity.Property(e => e.OldModifiedByUserId).HasColumnName("Old_ModifiedByUserId");

                entity.Property(e => e.OldNumberOfSapsubmissionAttempts).HasColumnName("Old_NumberOfSAPSubmissionAttempts");

                entity.Property(e => e.OldOrderSapNetValue)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("Old_OrderSapNetValue");

                entity.Property(e => e.OldOrderSapNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("Old_OrderSapNumber");

                entity.Property(e => e.OldOrderStatusId).HasColumnName("Old_OrderStatusId");

                entity.Property(e => e.OldOrderTypeId).HasColumnName("Old_OrderTypeId");

                entity.Property(e => e.OldPlantCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Old_PlantCode");

                entity.Property(e => e.OldPlantId).HasColumnName("Old_PlantId");

                entity.Property(e => e.OldShoppingCartId).HasColumnName("Old_ShoppingCartId");

                entity.Property(e => e.OldTruckSizeCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Old_TruckSizeCode");

                entity.Property(e => e.OldUserId).HasColumnName("Old_UserId");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.DmsOrdersChangeLogs)
                    .HasForeignKey(d => d.OrderId)
                    .HasConstraintName("FK_DmsOrders_ChangeLogs_DmsOrders");
            });

            modelBuilder.Entity<OrderStatus>(entity =>
            {
                entity.ToTable("OrderStatuses", "Orders");

                entity.HasIndex(e => e.Code, "IX_OrderStatuses")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_OrderStatuses_1")
                    .IsUnique();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<OrderType>(entity =>
            {
                entity.ToTable("OrderTypes", "Orders");

                entity.HasIndex(e => e.Code, "IX_OrderTypes")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_OrderTypes_1")
                    .IsUnique();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Otp>(entity =>
            {
                entity.ToTable("Otps", "Orders");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateExpiry).HasColumnType("datetime");

                entity.Property(e => e.EmailAddress)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.DmsOrderGroup)
                    .WithMany(p => p.Otps)
                    .HasForeignKey(d => d.DmsOrderGroupId)
                    .HasConstraintName("FK_Otps_DmsOrderGroup");

                entity.HasOne(d => d.DmsOrder)
                    .WithMany(p => p.Otps)
                    .HasForeignKey(d => d.DmsOrderId)
                    .HasConstraintName("FK_Otps_DmsOrders");

                entity.HasOne(d => d.OtpStatus)
                    .WithMany(p => p.Otps)
                    .HasForeignKey(d => d.OtpStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Otps_OtpStatuses");
            });

            modelBuilder.Entity<OtpStatus>(entity =>
            {
                entity.ToTable("OtpStatuses", "Orders");

                entity.HasIndex(e => e.Name, "IX_OtpStatuses_UniqueKey1")
                    .IsUnique();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Plant>(entity =>
            {
                entity.ToTable("Plants", "Orders");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.DateRefreshed).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.PlantTypeCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PlantType>(entity =>
            {
                entity.ToTable("PlantTypes", "Orders");

                entity.HasIndex(e => e.Code, "IX_PlantTypes_1")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_PlantTypes_2")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PlantsBk>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("plants_bk", "Orders");

                entity.Property(e => e.Code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.DateRefreshed).HasColumnType("datetime");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Name)
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.PlantTypeCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products", "Orders");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CompanyCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CountryCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ProductSapNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UnitOfMeasureCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ShoppingCart>(entity =>
            {
                entity.ToTable("ShoppingCarts", "Orders");

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.DeliveryMethod)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.PlantCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.DistributorSapAccount)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.DistributorSapAccountId)
                    .HasConstraintName("FK_ShoppingCarts_DistributorSapAccounts");

                entity.HasOne(d => d.ShoppingCartStatus)
                    .WithMany(p => p.ShoppingCarts)
                    .HasForeignKey(d => d.ShoppingCartStatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCarts_ShoppingCartStatuses");
            });

            modelBuilder.Entity<ShoppingCartItem>(entity =>
            {
                entity.ToTable("ShoppingCartItems", "Orders");

                entity.Property(e => e.ChannelCode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.DateCreated).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.DateOfOrderEstimate).HasColumnType("datetime");

                entity.Property(e => e.DeliveryCountryCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryMethodCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DeliveryStateCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PlantCode)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 5)");

                entity.Property(e => e.SapEstimatedOrderValue).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.UnitOfMeasureCode)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.DistributorSapAccount)
                    .WithMany(p => p.ShoppingCartItems)
                    .HasForeignKey(d => d.DistributorSapAccountId)
                    .HasConstraintName("FK_ShoppingCartItems_DistributorSapAccounts");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ShoppingCartItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartItems_Products");

                entity.HasOne(d => d.ShoppingCart)
                    .WithMany(p => p.ShoppingCartItems)
                    .HasForeignKey(d => d.ShoppingCartId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ShoppingCartItems_ShoppingCarts");
            });

            modelBuilder.Entity<ShoppingCartStatus>(entity =>
            {
                entity.ToTable("ShoppingCartStatuses", "Orders");

                entity.HasIndex(e => e.Code, "IX_ShoppingCartStatuses")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_ShoppingCartStatuses_1")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TripStatus>(entity =>
            {
                entity.ToTable("TripStatuses", "Orders");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TruckSize>(entity =>
            {
                entity.ToTable("TruckSizes", "Orders");

                entity.HasIndex(e => e.Code, "IX_TruckSizes")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_TruckSizes_1")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TruckSizeDeliveryMethodMapping>(entity =>
            {
                entity.ToTable("TruckSizeDeliveryMethodMappings", "Orders");

                entity.Property(e => e.DeliveryMethodCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.PlantTypeCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TruckSizeCode)
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UnitOfMeasure>(entity =>
            {
                entity.ToTable("UnitOfMeasures", "Orders");

                entity.HasIndex(e => e.Code, "IX_UnitOfMeasures")
                    .IsUnique();

                entity.HasIndex(e => e.Name, "IX_UnitOfMeasures_1")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
