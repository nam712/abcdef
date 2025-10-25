using Backend.Models;
using Microsoft.EntityFrameworkCore;
using YourShopManagement.API.Models;

namespace YourShopManagement.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ==================== DbSets cho 13 bảng ====================
        public DbSet<BusinessCategory> BusinessCategories { get; set; }
        public DbSet<ShopOwner> ShopOwners { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<MomoInfo> MomoInfos { get; set; }

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
                entity.Property(e => e.ShopName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.TermsAndConditionsAgreed).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.BusinessCategoryId);

                // Relationships
                entity.HasOne(e => e.BusinessCategory)
                    .WithMany(c => c.ShopOwners)
                    .HasForeignKey(e => e.BusinessCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ==================== 3. SUPPLIER ====================
            modelBuilder.Entity<Supplier>(b =>
            {
                b.ToTable("suppliers");

                b.HasKey(x => x.SupplierId);
                b.Property(x => x.SupplierId).HasColumnName("supplier_id");
                b.Property(x => x.SupplierCode).HasColumnName("supplier_code").IsRequired().HasMaxLength(50);
                b.Property(x => x.SupplierName).HasColumnName("supplier_name").IsRequired().HasMaxLength(255);
                b.Property(x => x.Address).HasColumnName("address");
                b.Property(x => x.Phone).HasColumnName("phone").HasMaxLength(20);
                b.Property(x => x.Email).HasColumnName("email").HasMaxLength(100);
                b.Property(x => x.TaxCode).HasColumnName("tax_code").HasMaxLength(50);
                b.Property(x => x.ContactName).HasColumnName("contact_person").HasMaxLength(100);
                b.Property(x => x.BankAccount).HasColumnName("bank_account").HasMaxLength(50);
                b.Property(x => x.BankName).HasColumnName("bank_name").HasMaxLength(100);
                b.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).HasDefaultValue("active");
                b.Property(x => x.Notes).HasColumnName("notes");
                b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                b.Property(x => x.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
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
                b.Property(x => x.ProductCode).HasColumnName("product_code").IsRequired().HasMaxLength(50);
                b.Property(x => x.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(255);
                b.Property(x => x.Description).HasColumnName("description");
                b.Property(x => x.CategoryId).HasColumnName("category_id");
                b.Property(x => x.Brand).HasColumnName("brand").HasMaxLength(100);
                b.Property(x => x.SupplierId).HasColumnName("supplier_id");
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

                b.HasOne(x => x.Supplier)
                 .WithMany(s => s.Products)
                 .HasForeignKey(x => x.SupplierId)
                 .OnDelete(DeleteBehavior.SetNull);

                b.HasOne(x => x.Category)
                 .WithMany(c => c.Products)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.SetNull);
            });
            // ==================== 6. PURCHASE ORDER ====================
            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.HasKey(e => e.PurchaseOrderId);

                entity.Property(e => e.PoCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SupplierId).IsRequired();
                entity.Property(e => e.PoDate).IsRequired();
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.PaymentStatus).HasMaxLength(20).HasDefaultValue("unpaid");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.PoCode).IsUnique();
                entity.HasIndex(e => e.SupplierId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PaymentStatus);
                entity.HasIndex(e => e.PoDate);

                // Relationships
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

            // ==================== 8. PRICE HISTORY ====================
            modelBuilder.Entity<PriceHistory>(entity =>
            {
                entity.HasKey(e => e.PriceHistoryId);

                entity.Property(e => e.OldPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NewPrice).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.EffectiveDate).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.EffectiveDate);

                // Relationships
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.PriceHistories)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
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
                entity.HasIndex(e => e.CustomerCode).IsUnique();
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

                entity.Property(e => e.EmployeeCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EmployeeName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.HireDate).IsRequired();
                entity.Property(e => e.Salary).HasColumnType("decimal(18,2)");
                entity.Property(e => e.WorkStatus).IsRequired().HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Indexes
                entity.HasIndex(e => e.EmployeeCode).IsUnique();
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
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Employee)
                    .WithMany(emp => emp.Invoices)
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.SetNull);

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
                entity.Property(e => e.DatePaid)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null
                    );

                // Cấu hình foreign key relationship
                entity.HasOne(m => m.PaymentMethod)
                    .WithMany()
                    .HasForeignKey(m => m.PaymentMethodId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        // ==================== Override SaveChanges để tự động cập nhật UpdatedAt ====================
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
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
            }
        }
    }
}