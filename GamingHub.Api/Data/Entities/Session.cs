namespace GamingHub.Api.Data.Entities
{
    public class Session
    {
        public Guid Id { get; set; }
        public required string LastRefreshTokenHash { get; set; }
        public DateTimeOffset InitiatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public string? Device {  get; set; }

        public required string UserId { get; set; }
        public GamingHubUser User { get; set; } = null!;
    }
}
