using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Reservations
{
    public record CreateReservationDto(
        [Required, StringLength(100, MinimumLength = 2)] string CustomerName,
        [Required, EmailAddress, StringLength(150)] string CustomerEmail,
        [Required] DateTimeOffset? StartTime,
        [Required] DateTimeOffset? EndTime);
}
