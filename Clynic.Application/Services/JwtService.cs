using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Clynic.Application.Interfaces.Services;
using Clynic.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Clynic.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationHours;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            
            _secretKey = _configuration["Jwt:SecretKey"] 
                ?? throw new InvalidOperationException("Jwt:SecretKey no está configurado");
            _issuer = _configuration["Jwt:Issuer"] ?? "ClynicAPI";
            _audience = _configuration["Jwt:Audience"] ?? "ClynicClients";
            _expirationHours = int.Parse(_configuration["Jwt:ExpirationHours"] ?? "24");
        }

        public string GenerarToken(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("IdClinica", usuario.IdClinica.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public DateTime ObtenerFechaExpiracion()
        {
            return DateTime.UtcNow.AddHours(_expirationHours);
        }
    }
}
