using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.DTOs.Auth;
using UniFlowPeople.Api.Models;
using UniFlowPeople.Api.Services.Auth;

namespace UniFlowPeople.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AppDbContext db,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var login = request.Login.Trim().ToLower();
        var usuario = await db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Ativo &&
                (x.Login.ToLower() == login || x.Email.ToLower() == login));

        if (usuario is null || !passwordService.Verify(request.Senha, usuario.SenhaHash))
            return Unauthorized(new { message = "Login ou senha invalidos." });

        var (token, expiresAt) = jwtTokenService.CreateToken(usuario);
        return ToResponse(usuario, token, expiresAt);
    }

    [HttpPost("bootstrap-system-admin")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> BootstrapSystemAdmin(BootstrapAdminRequest request)
    {
        var exists = await db.Usuarios.AnyAsync(x => x.Role == Roles.SistemaAdmin);
        if (exists) return Conflict(new { message = "O administrador do sistema ja foi criado." });

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Login = request.Login,
            Email = request.Email,
            SenhaHash = passwordService.Hash(request.Senha),
            Role = Roles.SistemaAdmin,
            Ativo = true
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        var (token, expiresAt) = jwtTokenService.CreateToken(usuario);
        return CreatedAtAction(nameof(Me), ToResponse(usuario, token, expiresAt));
    }

    [HttpGet("system-admin-exists")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> SystemAdminExists()
    {
        var exists = await db.Usuarios.AsNoTracking().AnyAsync(x => x.Role == Roles.SistemaAdmin);
        return Ok(new { exists });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioInfo>> Me()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var id)) return Unauthorized();

        var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.Ativo);
        return usuario is null ? Unauthorized() : ToInfo(usuario);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<UsuarioInfo>> UpdateMe(UpdateProfileRequest request)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var id)) return Unauthorized();

        var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.Ativo);
        if (usuario is null) return Unauthorized();

        usuario.Nome = request.Nome.Trim();
        usuario.Email = request.Email.Trim();
        await db.SaveChangesAsync();

        return ToInfo(usuario);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var id)) return Unauthorized();

        var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id && x.Ativo);
        if (usuario is null) return Unauthorized();

        if (!passwordService.Verify(request.SenhaAtual, usuario.SenhaHash))
            return BadRequest(new { message = "Senha atual invalida." });

        if (request.NovaSenha.Length < 6)
            return BadRequest(new { message = "A nova senha deve ter pelo menos 6 caracteres." });

        usuario.SenhaHash = passwordService.Hash(request.NovaSenha);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static AuthResponse ToResponse(Usuario usuario, string token, DateTime expiresAt) => new()
    {
        Token = token,
        ExpiresAt = expiresAt,
        Usuario = ToInfo(usuario)
    };

    private static UsuarioInfo ToInfo(Usuario usuario) => new()
    {
        Id = usuario.Id,
        EmpresaId = usuario.EmpresaId,
        ColaboradorId = usuario.ColaboradorId,
        Nome = usuario.Nome,
        Login = usuario.Login,
        Email = usuario.Email,
        Role = usuario.Role
    };
}
