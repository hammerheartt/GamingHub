using System.ComponentModel.DataAnnotations;

namespace GamingHub.Api.Data.Dtos.Auth
{
    public record RegisterRequestDto([Required, StringLength(100, MinimumLength = 1)] string UserName,
        [Required, StringLength(100, MinimumLength = 1), EmailAddress] string Email,
        [Required, StringLength(50, MinimumLength = 1)] string Password);
}
