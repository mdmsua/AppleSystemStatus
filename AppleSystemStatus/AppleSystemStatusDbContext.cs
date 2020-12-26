using AppleSystemStatus.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppleSystemStatus
{
    public class AppleSystemStatusDbContext : DbContext
    {
#nullable disable
        public DbSet<Store> Stores { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Event> Events { get; set; }
#nullable enable

        public AppleSystemStatusDbContext()
        {

        }

        public AppleSystemStatusDbContext(DbContextOptions<AppleSystemStatusDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Store>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(x => x.Name).IsUnique(true);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasMany(e => e.Services).WithOne(x => x.Store).HasForeignKey(x => x.StoreId).IsRequired(true);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(x => x.Name);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasMany(e => e.Events).WithOne(x => x.Service).HasForeignKey(x => x.ServiceId).IsRequired(true);
            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => new { e.ServiceId, e.EpochStartDate });
                entity.HasIndex(e => e.EpochEndDate);
                entity.HasOne(e => e.Service).WithMany(x => x.Events).HasForeignKey(x => x.ServiceId).IsRequired(true);
            });
        }
    }
}