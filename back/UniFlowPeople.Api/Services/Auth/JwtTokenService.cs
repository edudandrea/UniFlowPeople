using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UniFlowPeople.Api.Models;

namespace UniFlowPeople.Api.Services.Auth;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(Usuario usuario);
}

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(Usuario usuario)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key nao configurado.");
        var issuer = configuration["Jwt:Issuer"] ?? "UniFlowPeople";
        var audience = configuration["Jwt:Audience"] ?? "UniFlowPeople.Front";
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, usuario.Role),
            new("login", usuario.Login)
        };

        if (usuario.EmpresaId.HasValue) claims.Add(new Claim("empresaId", usuario.EmpresaId.Value.ToString()));
        if (usuario.ColaboradorId.HasValue) claims.Add(new Claim("colaboradorId", usuario.ColaboradorId.Value.ToString()));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
