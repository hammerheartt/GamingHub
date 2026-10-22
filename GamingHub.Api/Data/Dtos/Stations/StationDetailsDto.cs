using GamingHub.Api.Data.Dtos.Games;
using GamingHub.Api.Data.Dtos.Rooms;
using GamingHub.Api.Data.Entities;

namespace GamingHub.Api.Data.Dtos.Stations
{
    public record StationDetailsDto(
        int Id, string Name,
        string? Cpu, string? Gpu, int? RamGb,
        StationPlatform? Platform, int? Seats,
        string? Screen, bool IsActive,
        RoomDto Room,
        IEnumerable<GameDto> Games,
        IEnumerable<ReservationSlotDto> UpcomingReservations);
}