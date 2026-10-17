namespace GamingHub.Api.Data.Dtos.Games
{
    public record GameDto(int Id, string Title, string Genre, string? Publisher, DateOnly? ReleaseDate);
}
