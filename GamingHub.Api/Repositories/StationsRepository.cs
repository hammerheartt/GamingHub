using GamingHub.Api.Data;
using GamingHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Repositories
{
    public interface IStationsRepository
    {
        Task<IEnumerable<Station>> GetAll(int roomId);
        Task<Station?> Get(int roomId, int stationId);
        Task<Station> Create(Station station);
        Task<Station> Update(Station station);
        Task Delete(Station station);
        Task<bool> HasActiveReservations(int stationId);
        Task<Station?> GetWithGames(int roomId, int stationId);
        Task Save();
    }

    public class StationsRepository : IStationsRepository
    {
        private readonly GamingHubDbContext _db;
        public StationsRepository(GamingHubDbContext db) => _db = db;

        public async Task<IEnumerable<Station>> GetAll(int roomId) { return await _db.Stations.Where(o => o.RoomId == roomId).ToListAsync(); }

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
    }
}
