namespace GamingHub.Api.Data.Entities
{
    public enum ReservationStatus
    {
        Upcoming,
        Started,
        Finished,
        Cancelled,
        NoShow
    }
    public class Reservation
    {
        public int Id { get; set; }
        public required string CustomerName { get; set; }
        public required string CustomerEmail { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Upcoming;
        public DateTimeOffset CreatedAt { get; set; }
        public int StationId { get; set; }
        public Station Station { get; set; } = null!;
    }
}
