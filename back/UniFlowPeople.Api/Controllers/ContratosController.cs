using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.Models;
using UniFlowPeople.Api.Services.Auth;

namespace UniFlowPeople.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.SistemaAdmin)]
public class ContratosController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Contrato>>> GetAll(string? status = null)
    {
        var query = db.Contratos.AsNoTracking().Include(x => x.Empresa).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status.ToLower() == status.ToLower());

        return await query
            .OrderBy(x => x.DataFim ?? DateTime.MaxValue)
            .ToListAsync();
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> Dashboard()
    {
        var hoje = DateTime.UtcNow.Date;
        var em30Dias = hoje.AddDays(30);

        var contratos = await db.Contratos.AsNoTracking().ToListAsync();
        return new
        {
            empresasContratantes = await db.Empresas.CountAsync(x => x.Ativo),
            pagamentosEfetuados = contratos.Count(x => x.Status == "Pago" || x.Status == "Ativo"),
            pagamentosPendentes = contratos.Count(x => x.Status == "Pendente"),
            contratosAVencer = contratos.Count(x => x.DataFim.HasValue && x.DataFim.Value.Date >= hoje && x.DataFim.Value.Date <= em30Dias),
            contratosVencidos = contratos.Count(x => x.DataFim.HasValue && x.DataFim.Value.Date < hoje)
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Contrato>> GetById(int id)
    {
        var contrato = await db.Contratos.AsNoTracking().Include(x => x.Empresa).FirstOrDefaultAsync(x => x.Id == id);
        return contrato is null ? NotFound() : contrato;
    }

    [HttpPost]
    public async Task<ActionResult<Contrato>> Create(Contrato contrato)
    {
        NormalizarDatas(contrato);
        db.Contratos.Add(contrato);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = contrato.Id }, contrato);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Contrato contrato)
    {
        var current = await db.Contratos.FindAsync(id);
        if (current is null) return NotFound();

        NormalizarDatas(contrato);
        db.Entry(current).CurrentValues.SetValues(contrato);
        current.Id = id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contrato = await db.Contratos.FindAsync(id);
        if (contrato is null) return NotFound();

        db.Contratos.Remove(contrato);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static void NormalizarDatas(Contrato contrato)
    {
        contrato.DataInicio = DateTime.SpecifyKind(contrato.DataInicio, DateTimeKind.Utc);
        if (contrato.DataFim.HasValue)
            contrato.DataFim = DateTime.SpecifyKind(contrato.DataFim.Value, DateTimeKind.Utc);
    }
}
