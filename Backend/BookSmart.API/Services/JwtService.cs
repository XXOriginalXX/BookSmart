using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppointmentSystem.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace AppointmentSystem.API.Services
{
    public class JwtService
    {
        private readonly string _secret;
        private readonly int _expiryHours;

        public JwtService(IConfiguration config)
        {
            _secret = config["Jwt:Secret"] ?? throw new Exception("JWT secret not configured.");
            _expiryHours = int.Parse(config["Jwt:ExpiryHours"] ?? "12");
        }

        public string Generate(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_expiryHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}