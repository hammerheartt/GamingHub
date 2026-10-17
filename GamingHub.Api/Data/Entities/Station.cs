namespace GamingHub.Api.Data.Entities
{
    public enum StationPlatform
    {
        PlayStation,
        Xbox
    }

    public class Station
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // PC
        public string? Cpu { get; set; }
        public string? Gpu { get; set; }
        public int? RamGb { get; set; }

        // Consoles
        public StationPlatform? Platform { get; set; }
        public int? Seats { get; set; }

        // Both
        public string? Screen { get; set; }
        public bool IsActive { get; set; } = true;

        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public List<Reservation> Reservations { get; set; } = new();
    }
}
