using AppleSystemStatus.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppleSystemStatus
{
    public class AppleSystemStatusDbContext : DbContext
    {
#nullable disable
        public DbSet<Country> Countries { get; set; }

        public DbSet<Service> Services { get; set; }

        public DbSet<Event> Events { get; set; }
#nullable enable

        public AppleSystemStatusDbContext()
        {

        }

        public AppleSystemStatusDbContext(DbContextOptions<AppleSystemStatusDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=tcp:mdmsua.database.windows.net,1433;Initial Catalog=AppleSystemStatus;Persist Security Info=False;User ID=dmytro;Password=kQs9CbGWwW8NSLXG;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
                // optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("DatabaseConnectionString"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasMaxLength(8);
                entity.HasMany(e => e.Services).WithOne(x => x.Country).HasForeignKey(x => x.CountryId).IsRequired(true);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(x => x.Name);
                entity.HasIndex(e => new { e.Name, e.CountryId }).IsUnique(true);
                entity.Property(x => x.Name).HasMaxLength(96);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.CountryId).HasMaxLength(8);
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