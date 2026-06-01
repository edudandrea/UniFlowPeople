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
public class DocumentosColaboradoresController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentoColaborador>>> GetAll(int? colaboradorId = null)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var query = db.DocumentosColaboradores.AsNoTracking()
            .Where(x => x.Colaborador.EmpresaId == tenant.EmpresaId.Value);
        if (colaboradorId.HasValue) query = query.Where(x => x.ColaboradorId == colaboradorId);
        return await query.OrderByDescending(x => x.DataUpload).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<DocumentoColaborador>> Create(DocumentoColaborador documento)
    {
        if (!await ColaboradorPertenceEmpresa(documento.ColaboradorId)) return Forbid();
        db.DocumentosColaboradores.Add(documento);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = documento.Id }, documento);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        var current = await db.DocumentosColaboradores.Include(x => x.Colaborador).FirstOrDefaultAsync(x => x.Id == id);
        if (current is null) return NotFound();
        if (current.Colaborador.EmpresaId != tenant.EmpresaId) return Forbid();
        db.DocumentosColaboradores.Remove(current);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> ColaboradorPertenceEmpresa(int colaboradorId) =>
        tenant.EmpresaId.HasValue &&
        await db.Colaboradores.AnyAsync(x => x.Id == colaboradorId && x.EmpresaId == tenant.EmpresaId.Value);
}
