using ECommerce1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;

namespace ECommerce.Models
{
    public class ApplicationDbContext : DbContext
    {
        private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor? _httpContextAccessor;

        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
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
        public DbSet<AuditLog> AuditLogs { get; set; }

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

        public override int SaveChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges();
            OnAfterSaveChanges(auditEntries);
            return result;
        }

        public override async Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(cancellationToken, auditEntries);
            return result;
        }

        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name;
                auditEntries.Add(auditEntry);

                if (entry.State == EntityState.Added)
                {
                    auditEntry.Action = "Create";
                    foreach (var prop in entry.Properties)
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }
                else if (entry.State == EntityState.Deleted)
                {
                    auditEntry.Action = "Delete";
                    foreach (var prop in entry.Properties)
                    {
                        auditEntry.OldValues[prop.Metadata.Name] = prop.OriginalValue;
                    }
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditEntry.Action = "Update";
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            auditEntry.OldValues[prop.Metadata.Name] = prop.OriginalValue;
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }
                }
            }

            return auditEntries;
        }

        private Task OnAfterSaveChanges(System.Threading.CancellationToken cancellationToken, List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return Task.CompletedTask;

            var httpContext = _httpContextAccessor?.HttpContext;
            var userId = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                            ?? httpContext?.User?.Identity?.Name;

            foreach (var auditEntry in auditEntries)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserEmail = userEmail,
                    Action = auditEntry.Action,
                    TargetTable = auditEntry.TableName,
                    TargetId = auditEntry.GetTargetId(),
                    OldValues = auditEntry.OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(auditEntry.OldValues),
                    NewValues = auditEntry.NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(auditEntry.NewValues),
                    Timestamp = DateTime.UtcNow
                };
                AuditLogs.Add(auditLog);
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        private void OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0)
                return;

            var httpContext = _httpContextAccessor?.HttpContext;
            var userId = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value 
                            ?? httpContext?.User?.Identity?.Name;

            foreach (var auditEntry in auditEntries)
            {
                var auditLog = new AuditLog
                {
                    UserId = userId,
                    UserEmail = userEmail,
                    Action = auditEntry.Action,
                    TargetTable = auditEntry.TableName,
                    TargetId = auditEntry.GetTargetId(),
                    OldValues = auditEntry.OldValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(auditEntry.OldValues),
                    NewValues = auditEntry.NewValues.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(auditEntry.NewValues),
                    Timestamp = DateTime.UtcNow
                };
                AuditLogs.Add(auditLog);
            }

            base.SaveChanges();
        }
    }

    public class AuditEntry
    {
        public AuditEntry(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            Entry = entry;
        }

        public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; }
        public string TableName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();

        public string GetTargetId()
        {
            var keyValues = Entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue);
            return string.Join(";", keyValues);
        }
    }
}