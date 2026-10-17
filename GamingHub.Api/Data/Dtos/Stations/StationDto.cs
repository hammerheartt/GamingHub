using GamingHub.Api.Data.Entities;

namespace GamingHub.Api.Data.Dtos.Stations
{
    public record StationDto(
        int Id, int RoomId, string Name,
        string? Cpu, string? Gpu, int? RamGb,
        StationPlatform? Platform, int? Seats,
        string? Screen, bool IsActive);
}
