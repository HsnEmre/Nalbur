using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;

namespace Nalbur.Infrastructure.Data;

public class NalburDbContext : DbContext
{
    public NalburDbContext(DbContextOptions<NalburDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<InstallmentPlan> InstallmentPlans => Set<InstallmentPlan>();
    public DbSet<Installment> Installments => Set<Installment>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OutgoingPayment> OutgoingPayments => Set<OutgoingPayment>();
    public DbSet<WorkContract> WorkContracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // OutgoingPayment Configuration
        modelBuilder.Entity<OutgoingPayment>()
            .Property(op => op.Title).IsRequired().HasMaxLength(200);
        modelBuilder.Entity<OutgoingPayment>()
            .Property(op => op.Amount).HasPrecision(18, 2);
        modelBuilder.Entity<OutgoingPayment>()
            .Property(op => op.Category).HasMaxLength(100);

        // Product Configuration
        modelBuilder.Entity<Product>()
            .Property(p => p.CostPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Product>()
            .Property(p => p.SalePrice).HasPrecision(18, 2);

        // Customer Configuration
        modelBuilder.Entity<Customer>()
            .Property(c => c.Name).IsRequired().HasMaxLength(100);

        // Sale Configuration
        modelBuilder.Entity<Sale>()
            .Property(s => s.TotalAmount).HasPrecision(18, 2);
        
        modelBuilder.Entity<Sale>()
            .HasOne(s => s.Customer)
            .WithMany()
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // SaleItem Configuration
        modelBuilder.Entity<SaleItem>()
            .Property(si => si.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<SaleItem>()
            .Property(si => si.TotalPrice).HasPrecision(18, 2);

        modelBuilder.Entity<SaleItem>()
            .HasOne(si => si.Sale)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(si => si.SaleId);

        modelBuilder.Entity<SaleItem>()
            .HasOne(si => si.Product)
            .WithMany()
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // InstallmentPlan Configuration
        modelBuilder.Entity<InstallmentPlan>()
            .Property(ip => ip.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<InstallmentPlan>()
            .Property(ip => ip.DownPayment).HasPrecision(18, 2);

        modelBuilder.Entity<InstallmentPlan>()
            .HasOne(ip => ip.Sale)
            .WithOne(s => s.InstallmentPlan)
            .HasForeignKey<InstallmentPlan>(ip => ip.SaleId);

        // Installment Configuration
        modelBuilder.Entity<Installment>()
            .Property(i => i.Amount).HasPrecision(18, 2);
        
        modelBuilder.Entity<Installment>()
            .Property(i => i.PaidAmount).HasPrecision(18, 2)
            .HasDefaultValue(0);

        modelBuilder.Entity<Installment>()
            .HasOne(i => i.InstallmentPlan)
            .WithMany(ip => ip.Installments)
            .HasForeignKey(i => i.InstallmentPlanId);

        // Payment Configuration
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount).HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Installment)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InstallmentId);
    }
}
