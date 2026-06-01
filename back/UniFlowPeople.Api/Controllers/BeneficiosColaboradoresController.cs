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
public class BeneficiosColaboradoresController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BeneficioColaborador>>> GetAll(int? colaboradorId = null)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var query = db.BeneficiosColaboradores.AsNoTracking()
            .Where(x => x.Colaborador.EmpresaId == tenant.EmpresaId.Value);
        if (colaboradorId.HasValue) query = query.Where(x => x.ColaboradorId == colaboradorId);
        return await query.ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<BeneficioColaborador>> Create(BeneficioColaborador vinculo)
    {
        if (!await ColaboradorPertenceEmpresa(vinculo.ColaboradorId) || !await BeneficioPertenceEmpresa(vinculo.BeneficioId))
            return Forbid();

        db.BeneficiosColaboradores.Add(vinculo);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = vinculo.Id }, vinculo);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        var current = await db.BeneficiosColaboradores.Include(x => x.Colaborador).FirstOrDefaultAsync(x => x.Id == id);
        if (current is null) return NotFound();
        if (current.Colaborador.EmpresaId != tenant.EmpresaId) return Forbid();
        db.BeneficiosColaboradores.Remove(current);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> ColaboradorPertenceEmpresa(int colaboradorId) =>
        tenant.EmpresaId.HasValue &&
        await db.Colaboradores.AnyAsync(x => x.Id == colaboradorId && x.EmpresaId == tenant.EmpresaId.Value);

    private async Task<bool> BeneficioPertenceEmpresa(int beneficioId) =>
        tenant.EmpresaId.HasValue &&
        await db.Beneficios.AnyAsync(x => x.Id == beneficioId && x.EmpresaId == tenant.EmpresaId.Value);
}
