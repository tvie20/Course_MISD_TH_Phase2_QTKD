using Microsoft.EntityFrameworkCore;
using QTKD.Api.Models;

namespace QTKD.Api.Data
{
    public class QtkdDbContext : DbContext
    {
        public QtkdDbContext(DbContextOptions<QtkdDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<PriceList> PriceLists { get; set; }
        public DbSet<PriceItem> PriceItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Quan hệ Customer (1) ──── (N) Contract (Khóa ngoại: CustomerId, OnDelete: Restrict)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Code).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(255);
                entity.Property(c => c.Status).HasConversion<string>(); // Lưu enum dạng string trong DB (dễ đọc)

                entity.HasMany(c => c.Contracts)
                      .WithOne(ct => ct.Customer)
                      .HasForeignKey(ct => ct.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Customer nếu còn Contract

                entity.HasData(
                    new Customer
                    {
                        Id = 1,
                        Code = "KH00001",
                        Name = "Công ty TNHH Minh Phát",
                        Status = CustomerStatus.Active,
                        TaxCode = "0912345678",
                        Address = "123 Lê Lợi, Quận 1, TP.HCM"
                    },
                    new Customer
                    {
                        Id = 2,
                        Code = "KH00002",
                        Name = "Công ty Cổ phần An Khang",
                        Status = CustomerStatus.Active,
                        TaxCode = "0738297382",
                        Address = "456 Nguyễn Trãi, Quận 5, TP.HCM"
                    }
                );
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.HasKey(ct => ct.Id);
                entity.Property(ct => ct.ContractNo).IsRequired().HasMaxLength(100);
                entity.Property(ct => ct.Status).HasConversion<string>();
                entity.Property(ct => ct.TotalAmount).HasPrecision(18, 2);

                entity.HasData(
                    new Contract
                    {
                        Id = 1,
                        ContractNo = "HD-2026-018",
                        CustomerId = 1,
                        StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
                        EndDate = new DateTime(2027, 10, 31, 0, 0, 0, DateTimeKind.Utc),
                        Status = ContractStatus.Draft,
                        TotalAmount = 45000000m,
                        AttachmentPath = "/attachments/hd-2026-018-draft.pdf",
                        Description = "Hợp đồng cung cấp dịch vụ CNTT cho Công ty Minh Phát",
                        CreatedAt = new DateTime(2026, 10, 28, 0, 0, 0, DateTimeKind.Utc)
                    }
                );
            });

            // 2. Quan hệ PriceList (1) ──── (N) PriceItem (Khóa ngoại: PriceListId, OnDelete: Cascade)
            modelBuilder.Entity<PriceList>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(255);

                entity.HasMany(p => p.PriceItems)
                      .WithOne(pi => pi.PriceList)
                      .HasForeignKey(pi => pi.PriceListId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasData(
                    new PriceList
                    {
                        Id = 1,
                        Name = "Bảng giá dịch vụ 2026",
                        EffectiveDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                        IsActive = true
                    }
                );
            });

            modelBuilder.Entity<PriceItem>(entity =>
            {
                entity.HasKey(pi => pi.Id);
                entity.Property(pi => pi.ServiceCode).IsRequired().HasMaxLength(50);
                entity.Property(pi => pi.ServiceName).IsRequired().HasMaxLength(255);
                entity.Property(pi => pi.UnitPrice).HasPrecision(18, 2);

                entity.HasData(
                    new PriceItem
                    {
                        Id = 1,
                        PriceListId = 1,
                        ServiceCode = "SRV00001",
                        ServiceName = "Tư vấn và thiết kế giải pháp hệ thống",
                        UnitPrice = 15000000m
                    },
                    new PriceItem
                    {
                        Id = 2,
                        PriceListId = 1,
                        ServiceCode = "SRV00002",
                        ServiceName = "Triển khai phần mềm Quản trị Kinh doanh QTKD",
                        UnitPrice = 30000000m
                    }
                );
            });
        }
    }
}
