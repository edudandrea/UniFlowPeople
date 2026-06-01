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
[Authorize(Roles = Roles.EmpresaRoles)]
public class FeriasController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ferias>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();
        return await db.Ferias.AsNoTracking()
            .Where(x => x.Colaborador.EmpresaId == tenant.EmpresaId.Value)
            .OrderBy(x => x.DataInicio)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<Ferias>> Create(Ferias ferias)
    {
        if (!await ColaboradorPertenceEmpresa(ferias.ColaboradorId)) return Forbid();
        db.Ferias.Add(ferias);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = ferias.Id }, ferias);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Update(int id, Ferias ferias)
    {
        var current = await db.Ferias.Include(x => x.Colaborador).FirstOrDefaultAsync(x => x.Id == id);
        if (current is null) return NotFound();
        if (current.Colaborador.EmpresaId != tenant.EmpresaId || !await ColaboradorPertenceEmpresa(ferias.ColaboradorId)) return Forbid();
        db.Entry(current).CurrentValues.SetValues(ferias);
        current.Id = id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        var current = await db.Ferias.Include(x => x.Colaborador).FirstOrDefaultAsync(x => x.Id == id);
        if (current is null) return NotFound();
        if (current.Colaborador.EmpresaId != tenant.EmpresaId) return Forbid();
        db.Ferias.Remove(current);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> ColaboradorPertenceEmpresa(int colaboradorId) =>
        tenant.EmpresaId.HasValue &&
        await db.Colaboradores.AnyAsync(x => x.Id == colaboradorId && x.EmpresaId == tenant.EmpresaId.Value);
}
