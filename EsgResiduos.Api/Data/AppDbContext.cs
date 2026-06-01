using EsgResiduos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EsgResiduos.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<WasteType> WasteTypes { get; set; }
        public DbSet<CollectionPoint> CollectionPoints { get; set; }
        public DbSet<Destination> Destination { get; set; }
        public DbSet<CollectionAlert> CollectionAlerts { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WasteType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.WasteCategory).IsUnique();
                entity.Property(e => e.WasteCategory).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            modelBuilder.Entity<CollectionPoint>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CapacityKg).HasPrecision(12, 2);
                entity.Property(e => e.AlertVolumeKg).HasPrecision(12, 2);
                entity.Property(e => e.OccupiedVolumeKg).HasPrecision(12, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
            });

            modelBuilder.Entity<Collection>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VolumeKg).HasPrecision(12, 2);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.DestinationHistory).HasMaxLength(4000);
                entity.HasOne(e => e.CollectionPoint)
                      .WithMany(p => p.Collections)
                      .HasForeignKey(e => e.CollectionPointId);
                entity.HasOne(e => e.WasteType)
                      .WithMany(w => w.Collections)
                      .HasForeignKey(e => e.WasteTypeId);
            });

            modelBuilder.Entity<Destination>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DestinationName).IsRequired().HasMaxLength(120);
                entity.Property(e => e.ProcessingType).IsRequired().HasMaxLength(60);
                entity.Property(e => e.DestinatedVolumeKg).HasPrecision(12, 2);
                entity.HasOne(e => e.Collection).WithMany(c => c.Destinations).HasForeignKey(e => e.CollectionId);
            });

            modelBuilder.Entity<CollectionAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AlertType).IsRequired().HasMaxLength(30);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(300);
                entity.HasOne(e => e.CollectionPoint)
                      .WithMany(p => p.Alerts)
                      .HasForeignKey(e => e.CollectionPointId);
                entity.HasOne(e => e.Collection)
                      .WithMany(c => c.Alerts)
                      .HasForeignKey(e => e.CollectionId);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            });
        }
    }
}
