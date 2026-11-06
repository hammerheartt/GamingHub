using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Auth
{
    public record RefreshRequestDto([Required] string RefreshToken);
}
