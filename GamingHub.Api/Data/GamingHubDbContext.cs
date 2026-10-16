using GamingHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Data
{
    public class GamingHubDbContext : DbContext
    {
        public GamingHubDbContext(DbContextOptions<GamingHubDbContext> options) : base(options) { }

        public DbSet<Room> Rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>().Property(o => o.Type).HasConversion<string>();
            modelBuilder.Entity<Room>().Property(o => o.HourlyRate).HasPrecision(10, 2);
        }
    }
}