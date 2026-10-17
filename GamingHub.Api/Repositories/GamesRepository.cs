using GamingHub.Api.Data;
using GamingHub.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamingHub.Api.Repositories
{
    public interface IGamesRepository
    {
        Task<IEnumerable<Game>> GetAll();
        Task<IEnumerable<Game>> GetFiltered(string? genre, string? search);
        Task<IEnumerable<Game>> GetByStation(int stationId);
        Task<Game?> Get(int gameId);
        Task<bool> TitleExists(string title, int? excludeGameId = null);
        Task<Game> Create(Game game);
        Task<Game> Update(Game game);
        Task Delete(Game game);
    }

    public class GamesRepository : IGamesRepository
    {
        private readonly GamingHubDbContext _db;
        public GamesRepository(GamingHubDbContext db) { _db = db; }
        public async Task<IEnumerable<Game>> GetAll()
        {
            return await _db.Games.OrderBy(o => o.Title).ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetFiltered(string? genre, string? search)
        {
            var query = _db.Games.AsQueryable();
            if (!string.IsNullOrWhiteSpace(genre)) query = query.Where(g => g.Genre == genre);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(g => g.Title.Contains(search));
            return await query.OrderBy(g => g.Title).ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetByStation(int stationId)
        {
            return await _db.Games
                .Where(o => o.Stations.Any(o => o.Id == stationId))
                .OrderBy(o => o.Title)
                .ToListAsync();
        }
        public async Task<Game?> Get(int gameId) 
        { 
            return await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId); 
        }

        public async Task<bool> TitleExists(string title, int? excludeGameId = null)
        {
            return await _db.Games.AnyAsync(g => g.Title == title && (excludeGameId == null || g.Id != excludeGameId));
        }

        public async Task<Game> Create(Game game)
        {
            _db.Games.Add(game);
            await _db.SaveChangesAsync();
            return game;
        }

        public async Task<Game> Update(Game game)
        {
            _db.Games.Update(game);
            await _db.SaveChangesAsync();
            return game;
        }

        public async Task Delete(Game game)
        {
            _db.Games.Remove(game);
            await _db.SaveChangesAsync();
        }
    }
}
