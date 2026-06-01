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
public class CandidatosController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Candidato>>> GetAll(int? vagaId = null)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var query = db.Candidatos.AsNoTracking()
            .Where(x => x.Vaga.EmpresaId == tenant.EmpresaId.Value);
        if (vagaId.HasValue) query = query.Where(x => x.VagaId == vagaId);
        return await query.OrderByDescending(x => x.DataCadastro).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<Candidato>> Create(Candidato candidato)
    {
        if (!await VagaPertenceEmpresa(candidato.VagaId)) return Forbid();
        db.Candidatos.Add(candidato);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = candidato.Id }, candidato);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Update(int id, Candidato candidato)
    {
        var current = await db.Candidatos.Include(x => x.Vaga).FirstOrDefaultAsync(x => x.Id == id);
        if (current is null) return NotFound();
        if (current.Vaga.EmpresaId != tenant.EmpresaId || !await VagaPertenceEmpresa(candidato.VagaId)) return Forbid();
        db.Entry(current).CurrentValues.SetValues(candidato);
        current.Id = id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        var current = await db.Candidatos.Include(x => x.Vaga).FirstOrDefaultAsync(x => x.Id == id);
        if (current is null) return NotFound();
        if (current.Vaga.EmpresaId != tenant.EmpresaId) return Forbid();
        db.Candidatos.Remove(current);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> VagaPertenceEmpresa(int vagaId) =>
        tenant.EmpresaId.HasValue &&
        await db.Vagas.AnyAsync(x => x.Id == vagaId && x.EmpresaId == tenant.EmpresaId.Value);
}
