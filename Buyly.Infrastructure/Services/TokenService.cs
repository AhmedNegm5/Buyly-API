using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Buyly.Application.Interfaces;
using Buyly.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Buyly.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(User user, IList<string> roles)
        {
            var jwtSection = _configuration.GetSection("JwtSettings");
            if (!jwtSection.Exists())
            {
                throw new InvalidOperationException("JWT settings are missing from configuration.");
            }

            var secretKey = jwtSection["SecretKey"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var expirationValue = jwtSection["ExpirationMinutes"];

            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            if (string.IsNullOrWhiteSpace(issuer))
            {
                throw new InvalidOperationException("JWT Issuer is not configured.");
            }

            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new InvalidOperationException("JWT Audience is not configured.");
            }

            if (!double.TryParse(expirationValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var expirationMinutes))
            {
                throw new InvalidOperationException("JWT ExpirationMinutes is not configured with a valid number.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}