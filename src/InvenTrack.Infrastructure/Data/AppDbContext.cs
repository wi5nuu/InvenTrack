using InvenTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();
    public DbSet<AlertLog> AlertLogs => Set<AlertLog>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.PasswordSalt).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.Property(e => e.SKU).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Unit).HasMaxLength(20).HasDefaultValue("pcs");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.Property(e => e.MovementType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Reference).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(u => u.StockMovements)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ImportJobs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AlertLog>(entity =>
        {
            entity.Property(e => e.AlertType).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.SentTo).HasMaxLength(500);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.AlertLogs)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
