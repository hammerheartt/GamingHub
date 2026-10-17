using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Games
{
    public record UpdateGameDto(
        [Required, StringLength(150, MinimumLength = 1)] string Title,
        [Required, StringLength(50, MinimumLength = 2)] string Genre,
        [StringLength(100)] string? Publisher,
        DateOnly? ReleaseDate);
}
