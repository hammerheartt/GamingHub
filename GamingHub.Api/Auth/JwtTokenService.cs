using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace GamingHub.Api.Auth
{
    public class JwtTokenService
    {
        private readonly SymmetricSecurityKey _authSigningKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _refreshAudience;
        private readonly int _accessTokenLifeMinutes;
        private readonly int _refreshTokenLifeHours;
        public JwtTokenService(IConfiguration configuration) 
        {
            _authSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!));
            _issuer = configuration["Jwt:ValidIssuer"]!;
            _audience = configuration["Jwt:ValidAudience"]!;
            _refreshAudience = configuration["Jwt:RefreshAudience"]!;
            _accessTokenLifeMinutes = int.Parse(configuration["Jwt:AccessTokenLifeMinutes"]!);
            _refreshTokenLifeHours = int.Parse(configuration["Jwt:RefreshTokenLifeHours"]!);
        }

        public string CreateAccessToken(string userId, string username, IEnumerable<string> roles)
        {
            var authClaims = new List<Claim>()
            {
                new (ClaimTypes.Name, username),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId)
            };

            authClaims.AddRange(roles.Select(o => new Claim(ClaimTypes.Role, o)));

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: authClaims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenLifeMinutes),
                signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateRefreshToken(string userId)
        {
            var authClaims = new List<Claim>()
            {
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId)
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _refreshAudience,
                claims: authClaims,
                expires: DateTime.UtcNow.AddHours(_refreshTokenLifeHours),
                signingCredentials: new SigningCredentials(_authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool TryParseRefreshToken(string refreshToken, out ClaimsPrincipal? claims) 
        {
            claims = null;

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler
                {
                    MapInboundClaims = false
                };

                var validationParameters = new TokenValidationParameters
                {
                    ValidIssuer = _issuer,
                    ValidAudience = _refreshAudience,
                    IssuerSigningKey = _authSigningKey,
                    ValidateLifetime = true,
                };

                claims = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
