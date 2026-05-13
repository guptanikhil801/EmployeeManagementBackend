using EmployeeManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Data
{
    public class AppDBContext(DbContextOptions<AppDBContext> options) : DbContext(options)
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Total)
                      .HasPrecision(18, 2)
                      .IsRequired();

                entity.Property(o => o.Status)
                      .HasMaxLength(20)
                      .HasDefaultValue("Pending")
                      .IsRequired();

                entity.Property(o => o.EmployeeId)
                      .IsRequired();

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Order_Status",
                    "Status IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled', 'Returned')"
                ));
            });
        }
    }
}
