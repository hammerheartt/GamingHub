using GamingHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Data
{
    public class GamingHubDbContext : DbContext
    {
        public GamingHubDbContext(DbContextOptions<GamingHubDbContext> options) : base(options) { }

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Station> Stations { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>().Property(o => o.Type).HasConversion<string>();
            modelBuilder.Entity<Room>().Property(o => o.HourlyRate).HasPrecision(10, 2);

            modelBuilder.Entity<Station>().Property(o => o.Platform).HasConversion<string>();
            modelBuilder.Entity<Station>()
                .HasOne(o => o.Room)
                .WithMany(o => o.Stations)
                .HasForeignKey(o => o.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reservation>().Property(o => o.Status).HasConversion<string>();
            modelBuilder.Entity<Reservation>().Property(o => o.TotalPrice).HasPrecision(10, 2);
            modelBuilder.Entity<Reservation>()
                .HasOne(o => o.Station)
                .WithMany(o => o.Reservations)
                .HasForeignKey(o => o.StationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}