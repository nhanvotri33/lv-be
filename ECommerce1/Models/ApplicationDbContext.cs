using ECommerce1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;

namespace ECommerce.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- DbSet Definitions ---

        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShippingInfo> ShippingInfos { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionUsage> PromotionUsages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<Province> Provinces { get; set; }
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=SSG-U4-Clear;Initial Catalog=csdl_phone;Integrated Security=True;Encrypt=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed OrderStatus
            modelBuilder.Entity<OrderStatus>().HasData(
                new OrderStatus { Id = 1, Name = "Pending", Description = "Chờ thanh toán" },
                new OrderStatus { Id = 2, Name = "Processing", Description = "Đang xử lý" },
                new OrderStatus { Id = 3, Name = "Shipping", Description = "Đang giao hàng" },
                new OrderStatus { Id = 4, Name = "Completed", Description = "Đã giao" },
                new OrderStatus { Id = 5, Name = "Cancelled", Description = "Đã hủy" },
                new OrderStatus { Id = 6, Name = "Return_failed", Description = "Giao thất bại/ Hoàn hàng" },
                new OrderStatus { Id = 7, Name = "Refunded", Description = "Đổi trả/ Hoàn tiền" }
            );

            // 1. DECIMAL PRECISION
            modelBuilder.Entity<Product>().Property(p => p.BasePrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ProductVariant>().Property(pv => pv.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>().Property(oi => oi.PriceAtPurchase).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Promotion>().Property(p => p.DiscountValue).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");

            // 2. INDEXES AND STRING CONSTRAINTS
            modelBuilder.Entity<Product>(entity => {
                entity.HasIndex(p => p.Slug).IsUnique();
                entity.Property(p => p.Slug).HasMaxLength(255).IsRequired();
            });

            modelBuilder.Entity<Category>(entity => {
                entity.HasIndex(c => c.Slug).IsUnique();
                entity.Property(c => c.Slug).HasMaxLength(255).IsRequired();
            });

            modelBuilder.Entity<Brand>(entity => {
                entity.HasIndex(b => b.Slug).IsUnique();
                entity.Property(b => b.Slug).HasMaxLength(255).IsRequired();
            });



            // 3. RELATIONSHIPS & CASCADE BEHAVIOR

            // --- Order & Promotion Fix ---
            modelBuilder.Entity<Order>(entity => {
                // FORCE PromotionId to be Nullable in the Database
                entity.Property(o => o.PromotionId).IsRequired(false);

                entity.HasOne(o => o.Promotion)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(o => o.PromotionId)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);

            });



            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category).WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand).WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(pv => pv.ProductId).OnDelete(DeleteBehavior.Cascade);

            // --- Items & Cart ---
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart).WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order).WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariant).WithMany(pv => pv.OrderItems)
                .HasForeignKey(oi => oi.VariantId).OnDelete(DeleteBehavior.Restrict);

            // --- General ---


            modelBuilder.Entity<PromotionUsage>()
                .HasOne(pu => pu.Promotion).WithMany(p => p.PromotionUsages)
                .HasForeignKey(pu => pu.PromotionId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(x => x.Username).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();

                entity.Property(x => x.Username).IsRequired().HasMaxLength(50);
                entity.Property(x => x.Email).IsRequired().HasMaxLength(100);
                entity.Property(x => x.PasswordHash).IsRequired();
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(x => x.Token).IsUnique();

                entity.HasOne(x => x.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(x => x.UserId);
            });

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}