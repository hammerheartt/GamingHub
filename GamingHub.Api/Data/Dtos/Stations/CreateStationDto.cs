using GamingHub.Api.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Stations
{
    public record CreateStationDto(
        [Required, StringLength(50, MinimumLength = 1)] string Name,
        [StringLength(100)] string? Cpu,
        [StringLength(100)] string? Gpu,
        [Range(1, 1024)] int? RamGb,
        StationPlatform? Platform,
        [Range(1, 10)] int? Seats,
        [StringLength(100)] string? Screen,
        bool? IsActive);
}
