using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Reservations
{
    public record UpdateReservationDto(
        [Required] DateTimeOffset? StartTime,
        [Required] DateTimeOffset? EndTime);
}
