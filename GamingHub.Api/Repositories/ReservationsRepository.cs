using GamingHub.Api.Data;
using GamingHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Repositories
{
    public interface IReservationsRepository
    {
        Task<IEnumerable<Reservation>> GetAll(int stationId);
        Task<IEnumerable<Reservation>> GetFiltered(DateOnly? date, ReservationStatus? status, int? roomId, string? customerEmail);
        Task<Reservation?> Get(int stationId, int reservationId);
        Task<bool> HasOverlap(int stationId, DateTimeOffset start, DateTimeOffset end, int? excludeReservationId = null);
        Task<Reservation> Create(Reservation reservation);
        Task<Reservation> Update(Reservation reservation);
        Task Delete(Reservation reservation);
    }

    public class ReservationsRepository : IReservationsRepository
    {
        private readonly GamingHubDbContext _db;
        public ReservationsRepository(GamingHubDbContext db) => _db = db;

        public async Task<IEnumerable<Reservation>> GetAll(int stationId)
        {
            return await _db.Reservations
                    .Where(o => o.StationId == stationId)
                    .OrderBy(o => o.StartTime)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetFiltered(DateOnly? date, ReservationStatus? status, int? roomId, string? customerEmail)
        {
            var query = _db.Reservations.AsQueryable();

            if (date != null)
            {
                var vilnius = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vilnius");
                var localMidnight = date.Value.ToDateTime(TimeOnly.MinValue);
                var from = new DateTimeOffset(localMidnight, vilnius.GetUtcOffset(localMidnight));
                var to = from.AddDays(1);
                query = query.Where(o => o.StartTime < to && o.EndTime > from);
            }
            if (status != null) { query = query.Where(o => o.Status == status); }
            if (roomId != null) { query = query.Where(o => o.Station.RoomId == roomId); }
            if (!string.IsNullOrEmpty(customerEmail)) { query = query.Where(o => o.CustomerEmail == customerEmail); }

            return await query.OrderBy(o => o.StartTime).ToListAsync();
        }

        public async Task<Reservation?> Get(int stationId, int reservationId)
        {
            return await _db.Reservations.FirstOrDefaultAsync(o => o.StationId == stationId && o.Id == reservationId);
        }

        public async Task<bool> HasOverlap(int stationId, DateTimeOffset start, DateTimeOffset end, int? excludeReservationId = null)
        {
            return await _db.Reservations.AnyAsync(o =>
                            o.StationId == stationId &&
                            (excludeReservationId == null || o.Id != excludeReservationId) &&
                            (o.Status == ReservationStatus.Upcoming || o.Status == ReservationStatus.Started) &&
                            o.StartTime < end && start < o.EndTime);
        }

        public async Task<Reservation> Create(Reservation reservation)
        {
            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
            return reservation;
        }

        public async Task<Reservation> Update(Reservation reservation)
        {
            _db.Reservations.Update(reservation);
            await _db.SaveChangesAsync();
            return reservation;
        }

        public async Task Delete(Reservation reservation)
        {
            _db.Reservations.Remove(reservation);
            await _db.SaveChangesAsync();
        }
    }
}
