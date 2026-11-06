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
        private readonly SessionService _sessionService;

        public AuthController(UserManager<GamingHubUser> userManager, JwtTokenService jwtTokenService, SessionService sessionService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _sessionService = sessionService;
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
        public async Task<ActionResult<AccessTokenResponseDto>> Login(LoginRequestDto dto)
        {
            var user = await _userManager.FindByNameAsync(dto.UserName);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            {
                return Problem(detail: "Wrong username or password.", statusCode: 401);
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            var sessionId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var expiresAt = now.Add(_jwtTokenService.RefreshTokenLifeTime());

            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.UserName!, roles);
            var refreshToken = _jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

            var device = Request.Headers.UserAgent.ToString();
            if (device.Length > 256)
            {
                device = device[..256];
            }

            await _sessionService.CreateSessionAsync(sessionId, user.Id, refreshToken, now, expiresAt, device);

            if (user.ForceRelogin)
            {
                user.ForceRelogin = false;
                await _userManager.UpdateAsync(user);
            }

            SetRefreshTokenCookie(refreshToken, expiresAt);

            return Ok(new AccessTokenResponseDto(accessToken));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AccessTokenResponseDto>> Refresh()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return InvalidToken();
            }

            if (!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims))
            {
                return InvalidToken();
            }

            var userId = claims!.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (userId == null || !Guid.TryParse(claims!.FindFirstValue("SessionId"), out var sessionId))
            {
                return InvalidToken();
            }

            if (!await _sessionService.IsSessionValidAsync(sessionId, refreshToken))
            {
                return InvalidToken();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return InvalidToken();
            }

            if (user.ForceRelogin)
            {
                await _sessionService.InvalidateSessionAsync(sessionId);
                return InvalidToken();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var expiresAt = DateTimeOffset.UtcNow.Add(_jwtTokenService.RefreshTokenLifeTime());

            var accessToken = _jwtTokenService.CreateAccessToken(user.Id, user.UserName!, roles);
            refreshToken = _jwtTokenService.CreateRefreshToken(sessionId, user.Id, expiresAt);

            await _sessionService.ExtendSessionAsync(sessionId, refreshToken, expiresAt);

            SetRefreshTokenCookie(refreshToken, expiresAt);

            return Ok(new AccessTokenResponseDto(accessToken));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshToken))
            {
                return InvalidToken();
            }

            if (!_jwtTokenService.TryParseRefreshToken(refreshToken, out var claims) || 
                !Guid.TryParse(claims!.FindFirstValue("SessionId"), out var sessionId))
            {
                return InvalidToken();
            }

            await _sessionService.InvalidateSessionAsync(sessionId);
            Response.Cookies.Delete("RefreshToken", new CookieOptions { Path = "/api/auth" });
            return NoContent();
        }

        void SetRefreshTokenCookie(string refreshToken, DateTimeOffset expiresAt)
        {
            Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Expires = expiresAt,
                Path = "/api/auth"
                // Secure = true
            });
        }

        ActionResult InvalidToken() { return Problem(detail: "Invalid refresh token.", statusCode: 401); }
    }
}
