namespace GamingHub.Api.Data.Entities
{
    public class Game
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Genre { get; set; }
        public string? Publisher { get; set; }
        public DateOnly? ReleaseDate {  get; set; }

        public List<Station> Stations { get; set; } = new();
    }
}
