using Backend.Models;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace YourShopManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly int _currentShopOwnerId;

        // Constructor với IHttpContextAccessor - được DI sử dụng
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst("shop_owner_id")?.Value;
            if (int.TryParse(claim, out var id))
                _currentShopOwnerId = id;
            else
                _currentShopOwnerId = 0; // Default value
        }

        // ==================== DbSets cho 15 bảng ====================
        public DbSet<BusinessCategory> BusinessCategories { get; set; }
        public DbSet<ShopOwner> ShopOwners { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<MomoInfo> MomoInfos { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==================== 1. BUSINESS CATEGORY ====================
            modelBuilder.Entity<BusinessCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.CategoryName)
                    .IsUnique();
            });

            // ==================== 2. SHOP OWNER ====================

            modelBuilder.Entity<ShopOwner>(entity =>
            {
                entity.HasKey(e => e.ShopOwnerId);

                entity.Property(e => e.ShopOwnerName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.TermsAndConditionsAgreed).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);
            });

            // ==================== 2.5. SHOP ====================
            modelBuilder.Entity<Shop>(entity =>
            {
                entity.HasKey(e => e.ShopId);

                entity.Property(e => e.ShopCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ShopName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.ShopAddress).HasMaxLength(255);
                entity.Property(e => e.ShopPhone).HasMaxLength(20);
                entity.Property(e => e.ShopEmail).HasMaxLength(100);
                entity.Property(e => e.ManagerName).HasMaxLength(255);
                entity.Property(e => e.ManagerPhone).HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.ShopOwnerId);
                entity.HasIndex(e => e.ShopCode);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.BusinessCategoryId);

                // Relationships
                entity.HasOne(e => e.ShopOwner)
                    .WithMany(so => so.Shops)
                    .HasForeignKey(e => e.ShopOwnerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.BusinessCategory)
                    .WithMany(bc => bc.Shops)
                    .HasForeignKey(e => e.BusinessCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==================== 3. SUPPLIER ====================
            modelBuilder.Entity<Supplier>(b =>
            {
                b.ToTable("suppliers");

                b.HasKey(x => x.SupplierId);
                b.Property(x => x.SupplierId).HasColumnName("supplier_id");
                b.Property(x => x.ShopOwnerId).HasColumnName("shop_owner_id").IsRequired();
                b.Property(x => x.SupplierCode).HasColumnName("supplier_code").IsRequired().HasMaxLength(50);
                b.Property(x => x.SupplierName).HasColumnName("supplier_name").IsRequired().HasMaxLength(255);
                b.Property(x => x.Address).HasColumnName("address");
                b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
                b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
                b.Property(x => x.TaxCode).HasColumnName("tax_code").HasMaxLength(50);
                b.Property(x => x.ContactPerson).HasColumnName("contact_person").HasMaxLength(100);
                b.Property(x => x.BankAccount).HasColumnName("bank_account").HasMaxLength(50);
                b.Property(x => x.BankName).HasColumnName("bank_name").HasMaxLength(100);
                b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("active");
                b.Property(x => x.Notes).HasColumnName("notes");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                b.HasIndex(x => x.ShopOwnerId);
                b.HasIndex(x => new { x.SupplierCode, x.ShopOwnerId }).IsUnique();
            });

            // ========================= PRODUCT CATEGORIES =========================
            modelBuilder.Entity<ProductCategory>(b =>
            {
                b.ToTable("product_categories");

                b.HasKey(x => x.CategoryId);
                b.Property(x => x.CategoryId).HasColumnName("category_id");
                b.Property(x => x.CategoryName).HasColumnName("category_name").IsRequired().HasMaxLength(255);
                b.Property(x => x.Description).HasColumnName("description");
                b.Property(x => x.ParentCategoryId).HasColumnName("parent_category_id");
                b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("active");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Self reference
                b.HasOne(x => x.ParentCategory)
                 .WithMany(x => x.Children)
                 .HasForeignKey(x => x.ParentCategoryId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ========================= PRODUCTS =========================
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("products");

                b.HasKey(x => x.ProductId);
                b.Property(x => x.ProductId).HasColumnName("product_id");
                b.Property(x => x.ShopOwnerId).HasColumnName("shop_owner_id").IsRequired();
                b.Property(x => x.ProductCode).HasColumnName("product_code").IsRequired().HasMaxLength(50);
                b.Property(x => x.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(255);
                b.Property(x => x.Description).HasColumnName("description");
                b.Property(x => x.CategoryId).HasColumnName("category_id");
                b.Property(x => x.Brand).HasColumnName("brand").HasMaxLength(100);
                b.Property(x => x.SupplierName).HasColumnName("supplier_name").HasMaxLength(255);
                b.Property(x => x.Price).HasColumnName("price").HasPrecision(18, 2);
                b.Property(x => x.CostPrice).HasColumnName("cost_price").HasPrecision(18, 2);
                b.Property(x => x.Stock).HasColumnName("stock");
                b.Property(x => x.MinStock).HasColumnName("min_stock");
                b.Property(x => x.Sku).HasColumnName("sku").HasMaxLength(50);
                b.Property(x => x.Barcode).HasColumnName("barcode").HasMaxLength(50);
                b.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(20);
                b.Property(x => x.ImageUrl).HasColumnName("image_url");
                b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("active");
                b.Property(x => x.Weight).HasColumnName("weight");
                b.Property(x => x.Dimension).HasColumnName("dimension").HasMaxLength(100);
                b.Property(x => x.Notes).HasColumnName("notes");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                b.HasIndex(x => x.ShopOwnerId);
                b.HasIndex(x => new { x.ProductCode, x.ShopOwnerId }).IsUnique();

                // Relationships (không cần FK đến ShopOwner)
                b.HasOne(x => x.Category)
                 .WithMany(c => c.Products)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ==================== 6. PURCHASE ORDER ====================
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.PurchaseOrderId);

                entity.Property(e => e.ShopOwnerId).HasColumnName("shop_owner_id").IsRequired();
                entity.Property(e => e.PoCode).HasColumnName("po_code").IsRequired().HasMaxLength(50);
                entity.Property(e => e.SupplierId).HasColumnName("supplier_id").IsRequired();
                entity.Property(e => e.PoDate).HasColumnName("po_date").IsRequired();
                entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
                entity.Property(e => e.ActualDeliveryDate).HasColumnName("actual_delivery_date");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.Status).HasColumnName("status").IsRequired().HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.PaymentStatus).HasColumnName("payment_status").HasMaxLength(20).HasDefaultValue("unpaid");
                entity.Property(e => e.Notes).HasColumnName("notes");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.ShopOwnerId);
                entity.HasIndex(e => new { e.PoCode, e.ShopOwnerId }).IsUnique();
                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.PoDate);

                // Relationships (không cần FK đến ShopOwner)
                entity.HasOne(e => e.Supplier)
                    .WithMany(s => s.PurchaseOrders)
                    .HasForeignKey(e => e.SupplierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== 7. PURCHASE ORDER DETAIL ====================
            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.HasKey(e => e.PurchaseOrderDetailId);

                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.ImportPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.FinalAmount).IsRequired().HasColumnType("decimal(18,2)");

                // Indexes
                entity.HasIndex(e => e.PurchaseOrderId);
                entity.HasIndex(e => e.ProductId);

                // Relationships
                entity.HasOne(e => e.PurchaseOrder)
                    .WithMany(po => po.PurchaseOrderDetails)
                    .HasForeignKey(e => e.PurchaseOrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.PurchaseOrderDetails)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== 9. PAYMENT METHOD ====================
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.PaymentMethodId);

                entity.Property(e => e.MethodName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MethodCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.MethodName).IsUnique();
                entity.HasIndex(e => e.MethodCode).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // ==================== 10. CUSTOMER ====================
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);

                entity.Property(e => e.ShopOwnerId).IsRequired();
                entity.Property(e => e.CustomerCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CustomerType).IsRequired().HasMaxLength(20).HasDefaultValue("retail");
                entity.Property(e => e.TotalDebt).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.TotalPurchaseAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.TotalPurchaseCount).HasDefaultValue(0);
                entity.Property(e => e.LoyaltyPoints).HasDefaultValue(0);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.ShopOwnerId);
                entity.HasIndex(e => new { e.CustomerCode, e.ShopOwnerId }).IsUnique();
                entity.HasIndex(e => e.CustomerName);
                entity.HasIndex(e => e.Phone);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CustomerType);
                entity.HasIndex(e => e.Gender);
                entity.HasIndex(e => e.DateOfBirth);
                entity.HasIndex(e => e.Segment);
                entity.HasIndex(e => e.TotalPurchaseAmount);
            });

            // ==================== 11. EMPLOYEE ====================
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);

                entity.Property(e => e.ShopOwnerId).IsRequired();
                entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EmployeeName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.HireDate).IsRequired();
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
                entity.Property(e => e.WorkStatus).IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.ShopOwnerId);
                entity.HasIndex(e => new { e.EmployeeCode, e.ShopOwnerId }).IsUnique();
                entity.HasIndex(e => e.EmployeeName);
                entity.HasIndex(e => e.Phone);
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Position);
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.WorkStatus);
                entity.HasIndex(e => e.Gender);
                entity.HasIndex(e => e.DateOfBirth);
                entity.HasIndex(e => e.HireDate);
                entity.HasIndex(e => e.Username).IsUnique();

                // Check constraint: Password required if Username exists
                entity.ToTable(t => t.HasCheckConstraint(
                    "check_password_if_username_exists",
                    "(username IS NULL) OR (password IS NOT NULL)"
                ));
            });

            // ==================== 12. INVOICE ====================
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasKey(e => e.InvoiceId);

                entity.Property(e => e.InvoiceCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerId).IsRequired();
                entity.Property(e => e.InvoiceDate).IsRequired();
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.FinalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.AmountPaid).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20).HasDefaultValue("unpaid");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.InvoiceCode).IsUnique();
                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.EmployeeId);
                entity.HasIndex(e => e.InvoiceDate);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.PaymentMethodId);

                // Relationships
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Invoices)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Employee)
                    .WithMany(emp => emp.Invoices)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.PaymentMethod)
                    .WithMany(pm => pm.Invoices)
                    .HasForeignKey(e => e.PaymentMethodId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==================== 13. INVOICE DETAIL ====================
            modelBuilder.Entity<InvoiceDetail>(entity =>
            {
                entity.HasKey(e => e.InvoiceDetailId);

                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.LineTotal).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.InvoiceId);
                entity.HasIndex(e => e.ProductId);

                // Relationships
                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.InvoiceDetails)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany(p => p.InvoiceDetails)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
           
            // Cấu hình MomoInfo
            modelBuilder.Entity<MomoInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderId).IsRequired();
                entity.Property(e => e.OrderInfo).IsRequired();
                entity.Property(e => e.PaymentMethodId).IsRequired();
                entity.Property(e => e.DatePaid).IsRequired();

                entity.HasOne(m => m.PaymentMethod)
                      .WithMany()
                      .HasForeignKey(m => m.PaymentMethodId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==================== PROMOTION ====================
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.HasKey(e => e.PromotionId);

                entity.Property(e => e.PromotionCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PromotionName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.PromotionType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DiscountValue)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MinPurchaseAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.MaxDiscountAmount)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("active");

                entity.Property(e => e.UsageCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("NOW()");

                // Configure JSONB columns for PostgreSQL
                entity.Property(e => e.ApplicableProducts)
                    .HasColumnType("jsonb");

                entity.Property(e => e.ApplicableCustomers)
                    .HasColumnType("jsonb");

                // Indexes
                entity.HasIndex(e => e.InvoiceId)
                    .HasDatabaseName("idx_promotions_invoice_id");

                entity.HasIndex(e => e.ShopOwnerId)
                    .HasDatabaseName("idx_promotions_shop_owner_id");

                entity.HasIndex(e => e.Status)
                    .HasDatabaseName("idx_promotions_status");

                entity.HasIndex(e => e.StartDate)
                    .HasDatabaseName("idx_promotions_start_date");

                entity.HasIndex(e => e.EndDate)
                    .HasDatabaseName("idx_promotions_end_date");

                // Unique constraint
                entity.HasIndex(e => new { e.PromotionCode, e.ShopOwnerId })
                    .IsUnique()
                    .HasDatabaseName("uq_promotions_code_shopowner");

                // Foreign key to Invoice
                entity.HasOne(e => e.Invoice)
                    .WithMany(i => i.Promotions)
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        // ==================== Override SaveChanges để tự động cập nhật UpdatedAt ====================
        public override int SaveChanges()
        {
            ConvertDateTimesToUtc();
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc();
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added && entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
                if (entry.State == EntityState.Added && entry.Entity.GetType().GetProperty("ShopOwnerId") != null)
                {
                    entry.Property("ShopOwnerId").CurrentValue = _currentShopOwnerId;
                }

            }
        }

    
        /// <summary>
        /// ✅ Helper method: Tự động chuyển tất cả DateTime properties sang UTC trước khi save
        /// </summary>
        private void ConvertDateTimesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        if (property.CurrentValue != null)
                        {
                            var value = (DateTime)property.CurrentValue;
                            if (value.Kind == DateTimeKind.Unspecified)
                            {
                                property.CurrentValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                            }
                            else if (value.Kind == DateTimeKind.Local)
                            {
                                property.CurrentValue = value.ToUniversalTime();
                            }
                        }
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?))
                    {
                        var nullableValue = (DateTime?)property.CurrentValue;
                        if (nullableValue.HasValue)
                        {
                            var value = nullableValue.Value;
                            if (value.Kind == DateTimeKind.Unspecified)
                            {
                                property.CurrentValue = DateTime.SpecifyKind(value, DateTimeKind.Utc);
                            }
                            else if (value.Kind == DateTimeKind.Local)
                            {
                                property.CurrentValue = value.ToUniversalTime();
                            }
                        }
                    }
                }
            }
        }
    }
}