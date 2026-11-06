using GamingHub.Api.Data.Dtos.Auth;
using GamingHub.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GamingHub.Api.Auth
{
    [ApiController]
    [Route("/api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<GamingHubUser> _userManager;
        private readonly JwtTokenService _jwtTokenService;

        public AuthController(UserManager<GamingHubUser> userManager, JwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register(RegisterRequestDto dto)
        {
            if (await _userManager.FindByNameAsync(dto.UserName) != null)
            {
                return Problem(detail: "Username is already taken.", statusCode: 409);
            }

            var user = new GamingHubUser { UserName = dto.UserName, Email = dto.Email };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem(ModelState);
            }

            await _userManager.AddToRoleAsync(user, GamingHubRoles.Member);

            return StatusCode(201, new RegisterResponseDto(user.Id, user.UserName!, user.Email!));
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Problem(detail: "Wrong username or password.", statusCode: 401);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.UserName!, roles);
            var refreshToken = _jwtTokenService.CreateRefreshToken(user.Id);
            return Ok(new TokenResponseDto(accessToken, refreshToken));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenResponseDto>> Refresh(RefreshRequestDto dto)
        {
            if (!_jwtTokenService.TryParseRefreshToken(dto.RefreshToken, out var claims))
            {
                return InvalidToken();
            }

            var userId = claims!.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null)
            {
                return InvalidToken();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return InvalidToken();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.UserName!, roles);
            var refreshToken = _jwtTokenService.CreateRefreshToken(user.Id);
            return Ok(new TokenResponseDto(accessToken, refreshToken));
        }

        ActionResult InvalidToken() { return Problem(detail: "Invalid refresh token.", statusCode: 401); }
    }
}
