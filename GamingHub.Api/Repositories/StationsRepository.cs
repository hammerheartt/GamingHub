using GamingHub.Api.Data;
using GamingHub.Api.Data.Entities;
using GamingHub.Api.Data.Paging;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Repositories
{
    public interface IStationsRepository
    {
        Task<PagedList<Station>> GetAll(int roomId, SearchParameters searchParameters);
        Task<Station?> Get(int roomId, int stationId);
        Task<Station> Create(Station station);
        Task<Station> Update(Station station);
        Task Delete(Station station);
        Task<bool> HasActiveReservations(int stationId);
        Task<Station?> GetWithGames(int roomId, int stationId);
        Task Save();

        Task<Station?> GetDetails(int roomId, int stationId);
    }

    public class StationsRepository : IStationsRepository
    {
        private readonly GamingHubDbContext _db;
        public StationsRepository(GamingHubDbContext db) => _db = db;

        public async Task<PagedList<Station>> GetAll(int roomId, SearchParameters searchParameters) 
        {
            var query = _db.Stations.Where(o => o.RoomId == roomId).OrderBy(o => o.Id);
            return await PagedList<Station>.CreateAsync(query, searchParameters.PageNumber!.Value, searchParameters.PageSize!.Value);
        }

        public async Task<Station?> Get(int roomId, int stationId) { return await _db.Stations.FirstOrDefaultAsync(o => o.RoomId == roomId && o.Id == stationId); }
            
        public async Task<Station> Create(Station station)
        {
            _db.Stations.Add(station);
            await _db.SaveChangesAsync();
            return station;
        }

        public async Task<Station> Update(Station station)
        {
            _db.Stations.Update(station);
            await _db.SaveChangesAsync();
            return station;
        }

        public async Task Delete(Station station)
        {
            _db.Stations.Remove(station);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> HasActiveReservations(int stationId)
        {
            return await _db.Reservations.AnyAsync(o => o.StationId == stationId &&
                (o.Status == ReservationStatus.Upcoming || o.Status == ReservationStatus.Started));
        }

        public async Task<Station?> GetWithGames(int roomId, int stationId)
        {
            return await _db.Stations.Include(o => o.Games).FirstOrDefaultAsync(o => o.RoomId == roomId && o.Id == stationId);
        }

        public async Task Save() { await _db.SaveChangesAsync(); }

        public async Task<Station?> GetDetails(int roomId, int stationId)
        {
            var now = DateTimeOffset.UtcNow;
            return await _db.Stations
                .Include(o => o.Room)
                .Include(o => o.Games.OrderBy(g => g.Title))
                .Include(o => o.Reservations
                    .Where(r => r.EndTime > now && (r.Status == ReservationStatus.Upcoming || r.Status == ReservationStatus.Started))
                    .OrderBy(r => r.StartTime))
                .FirstOrDefaultAsync(o => o.RoomId == roomId && o.Id == stationId);
        }
    }
}
