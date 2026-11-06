using GamingHub.Api.Data;
using GamingHub.Api.Data.Entities;
using System.Security.Cryptography;
using System.Text;

namespace GamingHub.Api.Auth
{
    public class SessionService
    {
        private readonly GamingHubDbContext _db;

        public SessionService(GamingHubDbContext db) 
        { 
            _db = db;
        }

        public async Task CreateSessionAsync(Guid sessionId, string userId, string refreshToken, 
                                             DateTimeOffset initiatedAt, DateTimeOffset expiresAt, string? device)
        {
            _db.Sessions.Add(new Session
            {
                Id = sessionId,
                UserId = userId,
                LastRefreshTokenHash = HashToken(refreshToken),
                InitiatedAt = initiatedAt,
                ExpiresAt = expiresAt,
                Device = device

            });

            await _db.SaveChangesAsync();
        }

        public async Task<bool> IsSessionValidAsync(Guid sessionId, string refreshToken)
        {
            var session = await _db.Sessions.FindAsync(sessionId);
            if (session == null || session.IsRevoked || session.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return false;
            }

            if (session.LastRefreshTokenHash != HashToken(refreshToken))
            {
                session.IsRevoked = true;
                await _db.SaveChangesAsync();
                return false;
            }

            return true;
        }

        public async Task ExtendSessionAsync(Guid sessionId, string refreshToken, DateTimeOffset expiresAt)
        {
            var session = await _db.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                return;
            }

            session.LastRefreshTokenHash = HashToken(refreshToken);
            session.ExpiresAt = expiresAt;
            await _db.SaveChangesAsync();
        }

        public async Task InvalidateSessionAsync(Guid sessionId)
        {
            var session = await _db.Sessions.FindAsync(sessionId);
            if (session == null)
            {
                return;
            }

            session.IsRevoked = true;
            await _db.SaveChangesAsync();
        }


        private static string HashToken(string token)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}
