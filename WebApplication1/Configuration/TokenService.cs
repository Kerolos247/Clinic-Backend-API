using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1.Configuration
{
    public class TokenService
    {
        private readonly JWTOptions _jwtOptions;
        public TokenService(IOptions<JWTOptions> options)
        {
            _jwtOptions = options.Value;
        }
        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var keyBytes = Encoding.UTF8.GetBytes(_jwtOptions.SigninKey);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Lifetime),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}

