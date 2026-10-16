using GamingHub.Api.Data;
using GamingHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Repositories
{
    public interface IRoomsRepository
    {
        Task<IEnumerable<Room>> GetAll();
        Task<Room?> Get(int roomId);
        Task<Room> Create(Room room);
        Task<Room> Update(Room room);
        Task Delete(Room room);
    }

    public class RoomsRepository : IRoomsRepository
    {
        private readonly GamingHubDbContext _db;
        public RoomsRepository(GamingHubDbContext db) { _db = db; }

        public async Task<IEnumerable<Room>> GetAll() { return await _db.Rooms.ToListAsync(); }

        public async Task<Room?> Get(int roomId) { return await _db.Rooms.FirstOrDefaultAsync(o => o.Id == roomId); }

        public async Task<Room> Create(Room room)
        {
            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();
            return room;
        }

        public async Task<Room> Update(Room room)
        {
            _db.Rooms.Update(room);
            await _db.SaveChangesAsync();
            return room;
        }

        public async Task Delete(Room room)
        {
            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();
        }

    }
}
