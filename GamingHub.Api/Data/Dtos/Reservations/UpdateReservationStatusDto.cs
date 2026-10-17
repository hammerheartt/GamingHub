using GamingHub.Api.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Reservations
{
    public record UpdateReservationStatusDto([Required] ReservationStatus? Status);
}
