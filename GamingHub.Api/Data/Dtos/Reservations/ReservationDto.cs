using GamingHub.Api.Data.Entities;

namespace GamingHub.Api.Data.Dtos.Reservations
{
    public record ReservationDto(
        int Id, int StationId, string CustomerName, string CustomerEmail,
        DateTimeOffset StartTime, DateTimeOffset EndTime, decimal TotalPrice,
        ReservationStatus Status, DateTimeOffset CreatedAt);
}
