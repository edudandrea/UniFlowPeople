using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.Models;
using UniFlowPeople.Api.Services.Auth;
using UniFlowPeople.Api.Services.Tenancy;

namespace UniFlowPeople.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpresasController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<IEnumerable<Empresa>>> GetAll()
    {
        return await db.Empresas
            .AsNoTracking()
            .Include(x => x.Contratos.OrderByDescending(c => c.DataInicio).Take(1))
            .OrderBy(x => x.NomeFantasia)
            .ToListAsync();
    }

    [HttpGet("minha")]
    [Authorize(Roles = Roles.EmpresaRoles)]
    public async Task<ActionResult<Empresa>> MinhaEmpresa()
    {
        if (tenant.EmpresaId is null) return Forbid();
        var empresa = await db.Empresas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == tenant.EmpresaId);
        return empresa is null ? NotFound() : empresa;
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Empresa>> GetById(int id)
    {
        var empresa = await db.Empresas
            .AsNoTracking()
            .Include(x => x.Contratos)
            .FirstOrDefaultAsync(x => x.Id == id);

        return empresa is null ? NotFound() : empresa;
    }

    [HttpPost]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Empresa>> Create(Empresa empresa)
    {
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = empresa.Id }, empresa);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<IActionResult> Update(int id, Empresa empresa)
    {
        var current = await db.Empresas.FindAsync(id);
        if (current is null) return NotFound();

        db.Entry(current).CurrentValues.SetValues(empresa);
        current.Id = id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var empresa = await db.Empresas.FindAsync(id);
        if (empresa is null) return NotFound();

        db.Empresas.Remove(empresa);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
