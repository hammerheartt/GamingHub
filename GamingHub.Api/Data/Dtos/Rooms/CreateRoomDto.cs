using GamingHub.Api.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Rooms
{
    public record CreateRoomDto(
        [Required, StringLength(100, MinimumLength = 2)] string Name,
        [Required] RoomType? Type,
        string? Description,
        [Range(0.01, 1000)] decimal HourlyRate);
}
