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
public class RegistrosPontoController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RegistroPonto>>> GetAll(int? colaboradorId = null)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var query = db.RegistrosPonto.AsNoTracking()
            .Where(x => x.Colaborador.EmpresaId == tenant.EmpresaId.Value);
        if (colaboradorId.HasValue) query = query.Where(x => x.ColaboradorId == colaboradorId);
        return await query.OrderByDescending(x => x.DataHora).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<RegistroPonto>> Create(RegistroPonto registro)
    {
        if (!await ColaboradorPertenceEmpresa(registro.ColaboradorId)) return Forbid();
        db.RegistrosPonto.Add(registro);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = registro.Id }, registro);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        var registro = await db.RegistrosPonto.Include(x => x.Colaborador).FirstOrDefaultAsync(x => x.Id == id);
        if (registro is null) return NotFound();
        if (registro.Colaborador.EmpresaId != tenant.EmpresaId) return Forbid();
        db.RegistrosPonto.Remove(registro);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> ColaboradorPertenceEmpresa(int colaboradorId) =>
        tenant.EmpresaId.HasValue &&
        await db.Colaboradores.AnyAsync(x => x.Id == colaboradorId && x.EmpresaId == tenant.EmpresaId.Value);
}
