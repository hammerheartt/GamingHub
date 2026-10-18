using GamingHub.Api.Data;
using GamingHub.Api.Data.Entities;
using GamingHub.Api.Data.Paging;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Repositories
{
    public interface IReservationsRepository
    {
        Task<PagedList<Reservation>> GetAll(int stationId, SearchParameters searchParameters);
        Task<PagedList<Reservation>> GetFiltered(DateOnly? date, ReservationStatus? status, int? roomId, 
                                                 string? customerEmail, SearchParameters searchParameters);
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

        public async Task<PagedList<Reservation>> GetAll(int stationId, SearchParameters searchParameters)
        {
            var query = _db.Reservations
                            .Where(o => o.StationId == stationId)
                            .OrderBy(o => o.StartTime);

            return await PagedList<Reservation>.CreateAsync(query, searchParameters.PageNumber!.Value, searchParameters.PageSize!.Value);
        }

        public async Task<PagedList<Reservation>> GetFiltered(DateOnly? date, ReservationStatus? status, int? roomId, 
                                                              string? customerEmail, SearchParameters searchParameters)
        {
            var query = _db.Reservations.AsQueryable();

            if (date != null)
            {
                var vilnius = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vilnius");
                var localMidnight = date.Value.ToDateTime(TimeOnly.MinValue);
                var from = new DateTimeOffset(localMidnight, vilnius.GetUtcOffset(localMidnight)).ToUniversalTime();
                var to = from.AddDays(1);
                query = query.Where(o => o.StartTime < to && o.EndTime > from);
            }
            if (status != null) { query = query.Where(o => o.Status == status); }
            if (roomId != null) { query = query.Where(o => o.Station.RoomId == roomId); }
            if (!string.IsNullOrEmpty(customerEmail)) { query = query.Where(o => o.CustomerEmail == customerEmail); }
            query = query.OrderBy(o => o.StartTime);
            return await PagedList<Reservation>.CreateAsync(query, searchParameters.PageNumber!.Value, searchParameters.PageSize!.Value);
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
