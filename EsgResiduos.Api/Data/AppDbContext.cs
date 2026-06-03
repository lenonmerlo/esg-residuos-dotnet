using EsgResiduos.Api.Models;

using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<WasteType>? WasteTypes { get; set; }
    public DbSet<CollectionPoint>? CollectionPoints { get; set; }
    public DbSet<Collection>? Collections { get; set; }
    public DbSet<Destination>? Destinations { get; set; }
    public DbSet<CollectionAlert>? CollectionAlerts { get; set; }
    public DbSet<User>? Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        _ = modelBuilder.Entity<WasteType>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.HasIndex(e => e.WasteCategory).IsUnique();
            _ = entity.Property(e => e.WasteCategory).IsRequired().HasMaxLength(30);
            _ = entity.Property(e => e.Description).HasMaxLength(200);
        });

        _ = modelBuilder.Entity<CollectionPoint>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            _ = entity.Property(e => e.CapacityKg).HasPrecision(12, 2);
            _ = entity.Property(e => e.AlertVolumeKg).HasPrecision(12, 2);
            _ = entity.Property(e => e.OccupiedVolumeKg).HasPrecision(12, 2);
            _ = entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
        });

        _ = modelBuilder.Entity<Collection>(entity =>
        {
            _ = entity.ToTable("Collection");
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.VolumeKg).HasPrecision(12, 2);
            _ = entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            _ = entity.Property(e => e.DestinationHistory).HasMaxLength(4000);
            _ = entity.HasOne(e => e.CollectionPoint)
                  .WithMany(p => p.Collections)
                  .HasForeignKey(e => e.CollectionPointId);
            _ = entity.HasOne(e => e.WasteType)
                  .WithMany(w => w.Collections)
                  .HasForeignKey(e => e.WasteTypeId);
        });

        _ = modelBuilder.Entity<Destination>(entity =>
        {
            _ = entity.ToTable("Destination");
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.DestinationName).IsRequired().HasMaxLength(120);
            _ = entity.Property(e => e.ProcessingType).IsRequired().HasMaxLength(60);
            _ = entity.Property(e => e.DestinatedVolumeKg).HasPrecision(12, 2);
            _ = entity.HasOne(e => e.Collection).WithMany(c => c.Destinations).HasForeignKey(e => e.CollectionId);
        });

        _ = modelBuilder.Entity<CollectionAlert>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.AlertType).IsRequired().HasMaxLength(30);
            _ = entity.Property(e => e.Message).IsRequired().HasMaxLength(300);
            _ = entity.HasOne(e => e.CollectionPoint)
                  .WithMany(p => p.Alerts)
                  .HasForeignKey(e => e.CollectionPointId);
            _ = entity.HasOne(e => e.Collection)
                  .WithMany(c => c.Alerts)
                  .HasForeignKey(e => e.CollectionId);
        });

        _ = modelBuilder.Entity<User>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.HasIndex(e => e.Email).IsUnique();
            _ = entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            _ = entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            _ = entity.Property(e => e.PasswordHash).IsRequired();
            _ = entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
        });
    }
}