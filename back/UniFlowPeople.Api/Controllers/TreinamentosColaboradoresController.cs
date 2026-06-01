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
public class TreinamentosColaboradoresController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TreinamentoColaborador>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();
        return await db.TreinamentosColaboradores
            .AsNoTracking()
            .Include(x => x.Treinamento)
            .Include(x => x.Colaborador)
            .Where(x => x.Treinamento.EmpresaId == tenant.EmpresaId.Value)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<TreinamentoColaborador>> Create(TreinamentoColaborador vinculo)
    {
        if (!await TreinamentoPertenceEmpresa(vinculo.TreinamentoId) || !await ColaboradorPertenceEmpresa(vinculo.ColaboradorId))
            return Forbid();

        db.TreinamentosColaboradores.Add(vinculo);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = vinculo.Id }, vinculo);
    }

    [HttpPut("{id:int}/presenca")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Presenca(int id, TreinamentoColaborador request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var vinculo = await db.TreinamentosColaboradores
            .Include(x => x.Treinamento)
            .FirstOrDefaultAsync(x => x.Id == id && x.Treinamento.EmpresaId == tenant.EmpresaId.Value);
        if (vinculo is null) return NotFound();

        vinculo.Presente = request.Presente;
        vinculo.DataPresenca = request.Presente ? DateTime.UtcNow : null;
        vinculo.Status = request.Presente ? "Presente" : request.Status;
        vinculo.Observacoes = request.Observacoes;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var vinculo = await db.TreinamentosColaboradores
            .Include(x => x.Treinamento)
            .FirstOrDefaultAsync(x => x.Id == id && x.Treinamento.EmpresaId == tenant.EmpresaId.Value);
        if (vinculo is null) return NotFound();

        db.TreinamentosColaboradores.Remove(vinculo);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> TreinamentoPertenceEmpresa(int treinamentoId) =>
        tenant.EmpresaId.HasValue &&
        await db.Treinamentos.AnyAsync(x => x.Id == treinamentoId && x.EmpresaId == tenant.EmpresaId.Value);

    private async Task<bool> ColaboradorPertenceEmpresa(int colaboradorId) =>
        tenant.EmpresaId.HasValue &&
        await db.Colaboradores.AnyAsync(x => x.Id == colaboradorId && x.EmpresaId == tenant.EmpresaId.Value);
}
