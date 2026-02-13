using Microsoft.EntityFrameworkCore;
using TaxiSignalRBackend.WebAPI.Models;

namespace TaxiSignalRBackend.WebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<TaxiRequest> TaxiRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Driver>()
                .HasIndex(d => d.Email)
                .IsUnique();
        }
    }
}