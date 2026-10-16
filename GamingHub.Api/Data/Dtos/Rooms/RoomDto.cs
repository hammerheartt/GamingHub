using GamingHub.Api.Data.Entities;

namespace GamingHub.Api.Data.Dtos.Rooms
{
    public record RoomDto(int Id, string Name, RoomType Type, string? Description, decimal HourlyRate);
}
