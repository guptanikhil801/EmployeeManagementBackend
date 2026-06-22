using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Data
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<Product> Products => Set<Product>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ── Employee configuration ──────────────────────────────────────────
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employees");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.Department)
                      .IsRequired(false)
                      .HasMaxLength(50);

                entity.Property(e => e.Salary)
                      .HasColumnType("decimal(18,2)");

                // Index on Email (unique)
                entity.HasIndex(e => e.Email)
                      .IsUnique();

                // Global query filter — soft delete 
                entity.HasQueryFilter(e => e.IsActive);
            });

            // ── Order configuration ─────────────────────────────────────────────
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");

                entity.Property(e => e.ProductName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.TotalAmount)
                      .HasColumnType("decimal(18,2)");

                entity.Property(e => e.Status)
                      .HasMaxLength(20)
                      .HasDefaultValue("Pending");

                // Shadow property — not on the class, managed by EF
                entity.Property<DateTime>("CreatedAt")
                      .HasDefaultValueSql("GETUTCDATE()");

                // Owned entity — Address stored in Orders table (no join needed)
                entity.OwnsOne(o => o.ShippingAddress, addr =>
                {
                    addr.Property(a => a.Street).HasColumnName("ShipStreet").HasMaxLength(200);
                    addr.Property(a => a.City).HasColumnName("ShipCity").HasMaxLength(100);
                    addr.Property(a => a.State).HasColumnName("ShipState").HasMaxLength(50);
                    addr.Property(a => a.PostalCode).HasColumnName("ShipPostalCode").HasMaxLength(20);
                });

                // Relationship — 1 Employee : many Orders
                entity.HasOne(o => o.Employee)
                      .WithMany(e => e.Orders)
                      .HasForeignKey(o => o.EmployeeId)
                      .OnDelete(DeleteBehavior.Restrict);  // don't cascade-delete orders

                entity.HasMany(o => o.Products)
                      .WithMany(p => p.Orders)
                      .UsingEntity(j => j.ToTable("OrderProducts"));

                // Concurrency token — optimistic concurrency
                entity.Property<byte[]>("RowVersion")
                      .IsRowVersion();
            });

            modelBuilder.Entity<EmployeeProfile>(entity =>
            {
                entity.HasOne(p => p.Employee)            // Profile has one Employee
                      .WithOne(e => e.Profile)            // Employee has one Profile
                      .HasForeignKey<EmployeeProfile>(p => p.EmployeeId) // FK lives on Profile side
                      .OnDelete(DeleteBehavior.Cascade);  // delete profile when employee deleted
            });
        }
    }
}
