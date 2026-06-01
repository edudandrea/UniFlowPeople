using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.DTOs.Usuarios;
using UniFlowPeople.Api.DTOs.Auth;
using UniFlowPeople.Api.Models;
using UniFlowPeople.Api.Services.Auth;
using UniFlowPeople.Api.Services.Tenancy;

namespace UniFlowPeople.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.SistemaAdmin + "," + Roles.EmpresaAdmin)]
public class UsuariosController(
    AppDbContext db,
    ITenantContext tenant,
    IPasswordService passwordService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioInfo>>> GetAll()
    {
        var query = db.Usuarios.AsNoTracking().AsQueryable();
        if (!tenant.IsSistemaAdmin)
        {
            if (tenant.EmpresaId is null) return Forbid();
            query = query.Where(x => x.EmpresaId == tenant.EmpresaId.Value);
        }

        return await query
            .OrderBy(x => x.Nome)
            .Select(x => new UsuarioInfo
            {
                Id = x.Id,
                EmpresaId = x.EmpresaId,
                ColaboradorId = x.ColaboradorId,
                Nome = x.Nome,
                Login = x.Login,
                Email = x.Email,
                Role = x.Role
            })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioInfo>> Create(UsuarioCreateRequest request)
    {
        var role = ResolveRole(request.Role);
        if (role is null) return BadRequest(new { message = "Perfil invalido." });

        var empresaId = request.EmpresaId;
        if (tenant.IsSistemaAdmin)
        {
            if (role == Roles.SistemaAdmin) return BadRequest(new { message = "Use o bootstrap apenas para o admin do sistema." });
            if (empresaId is null) return BadRequest(new { message = "EmpresaId e obrigatorio para usuarios de empresa." });
        }
        else
        {
            if (tenant.EmpresaId is null) return Forbid();
            if (role == Roles.SistemaAdmin) return Forbid();
            empresaId = tenant.EmpresaId.Value;
        }

        var usuario = new Usuario
        {
            EmpresaId = empresaId,
            ColaboradorId = request.ColaboradorId,
            Nome = request.Nome,
            Login = request.Login,
            Email = request.Email,
            SenhaHash = passwordService.Hash(request.Senha),
            Role = role,
            Ativo = request.Ativo
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = usuario.Id }, ToInfo(usuario));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UsuarioUpdateRequest request)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
        if (usuario is null) return NotFound();
        if (!tenant.IsSistemaAdmin && usuario.EmpresaId != tenant.EmpresaId) return Forbid();

        var role = ResolveRole(request.Role);
        if (role is null || role == Roles.SistemaAdmin) return BadRequest(new { message = "Perfil invalido." });

        usuario.ColaboradorId = request.ColaboradorId;
        usuario.Nome = request.Nome;
        usuario.Email = request.Email;
        usuario.Role = role;
        usuario.Ativo = request.Ativo;
        if (!string.IsNullOrWhiteSpace(request.Senha))
            usuario.SenhaHash = passwordService.Hash(request.Senha);

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
        if (usuario is null) return NotFound();
        if (!tenant.IsSistemaAdmin && usuario.EmpresaId != tenant.EmpresaId) return Forbid();
        if (usuario.Role == Roles.SistemaAdmin) return Forbid();

        db.Usuarios.Remove(usuario);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static string? ResolveRole(string role) => role switch
    {
        Roles.EmpresaAdmin => Roles.EmpresaAdmin,
        Roles.RH => Roles.RH,
        Roles.Gestor => Roles.Gestor,
        Roles.Colaborador => Roles.Colaborador,
        _ => null
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
