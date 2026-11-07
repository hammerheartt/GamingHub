using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Reservations
{
    public record CreateReservationDto(
        [Required] DateTimeOffset? StartTime,
        [Required] DateTimeOffset? EndTime);
}
