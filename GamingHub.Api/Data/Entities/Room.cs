namespace GamingHub.Api.Data.Entities
{
    public enum RoomType
    {
        PC,
        Console
    }

    public class Room
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public RoomType Type { get; set; }
        public string? Description { get; set; }
        public decimal HourlyRate { get; set; }
    }
}
