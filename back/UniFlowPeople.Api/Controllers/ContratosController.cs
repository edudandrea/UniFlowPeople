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
public class ContratosController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<IEnumerable<Contrato>>> GetAll(string? status = null)
    {
        var query = db.Contratos.AsNoTracking()
            .Include(x => x.Empresa)
            .Include(x => x.PlanoCadastro)
            .Include(x => x.Cobrancas)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(x => x.Status.ToLower() == status.ToLower());

        return await query
            .OrderBy(x => x.DataFim ?? DateTime.MaxValue)
            .ToListAsync();
    }

    [HttpGet("dashboard")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<object>> Dashboard()
    {
        var hoje = DateTime.UtcNow.Date;
        var em30Dias = hoje.AddDays(30);

        var contratos = await db.Contratos.AsNoTracking().ToListAsync();
        var cobrancas = await db.Cobrancas.AsNoTracking().ToListAsync();
        return new
        {
            empresasContratantes = await db.Empresas.CountAsync(x => x.Ativo),
            pagamentosEfetuados = cobrancas.Count(x => x.Status == "Pago"),
            pagamentosPendentes = cobrancas.Count(x => x.Status == "Pendente"),
            contratosAVencer = contratos.Count(x => x.DataFim.HasValue && x.DataFim.Value.Date >= hoje && x.DataFim.Value.Date <= em30Dias),
            contratosVencidos = contratos.Count(x => x.DataFim.HasValue && x.DataFim.Value.Date < hoje)
        };
    }

    [HttpGet("planos")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<IEnumerable<Plano>>> GetPlanos()
    {
        return await db.Planos.AsNoTracking()
            .OrderBy(x => x.Nome)
            .ToListAsync();
    }

    [HttpGet("planos/{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Plano>> GetPlanoById(int id)
    {
        var plano = await db.Planos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return plano is null ? NotFound() : plano;
    }

    [HttpPost("planos")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Plano>> CreatePlano(Plano plano)
    {
        db.Planos.Add(plano);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPlanoById), new { id = plano.Id }, plano);
    }

    [HttpPut("planos/{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<IActionResult> UpdatePlano(int id, Plano plano)
    {
        var current = await db.Planos.FindAsync(id);
        if (current is null) return NotFound();

        db.Entry(current).CurrentValues.SetValues(plano);
        current.Id = id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("planos/{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<IActionResult> DeletePlano(int id)
    {
        var plano = await db.Planos.FindAsync(id);
        if (plano is null) return NotFound();

        db.Planos.Remove(plano);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("cobrancas")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<IEnumerable<Cobranca>>> GetCobrancas()
    {
        return await db.Cobrancas.AsNoTracking()
            .Include(x => x.Empresa)
            .Include(x => x.Contrato)
            .OrderByDescending(x => x.DataGeracao)
            .ToListAsync();
    }

    [HttpGet("cobrancas/{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Cobranca>> GetCobrancaById(int id)
    {
        var cobranca = await db.Cobrancas.AsNoTracking()
            .Include(x => x.Empresa)
            .Include(x => x.Contrato)
            .FirstOrDefaultAsync(x => x.Id == id);
        return cobranca is null ? NotFound() : cobranca;
    }

    [HttpPost("cobrancas")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Cobranca>> CreateCobranca(Cobranca cobranca)
    {
        NormalizarDatas(cobranca);
        db.Cobrancas.Add(cobranca);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCobrancaById), new { id = cobranca.Id }, cobranca);
    }

    [HttpPut("cobrancas/{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<IActionResult> UpdateCobranca(int id, Cobranca cobranca)
    {
        var current = await db.Cobrancas.FindAsync(id);
        if (current is null) return NotFound();

        NormalizarDatas(cobranca);
        db.Entry(current).CurrentValues.SetValues(cobranca);
        current.Id = id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("cobrancas/{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<IActionResult> DeleteCobranca(int id)
    {
        var cobranca = await db.Cobrancas.FindAsync(id);
        if (cobranca is null) return NotFound();

        db.Cobrancas.Remove(cobranca);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("minha")]
    [Authorize(Roles = Roles.EmpresaAdmin)]
    public async Task<ActionResult<Contrato?>> MinhaContratacao()
    {
        if (tenant.EmpresaId is null) return Forbid();

        return await db.Contratos.AsNoTracking()
            .Include(x => x.PlanoCadastro)
            .Include(x => x.Cobrancas)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value && x.Status == "Ativo")
            .OrderByDescending(x => x.DataInicio)
            .FirstOrDefaultAsync();
    }

    [HttpGet("cobrancas/minhas")]
    [Authorize(Roles = Roles.EmpresaAdmin)]
    public async Task<ActionResult<IEnumerable<Cobranca>>> MinhasCobrancas()
    {
        if (tenant.EmpresaId is null) return Forbid();

        return await db.Cobrancas.AsNoTracking()
            .Include(x => x.Contrato)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.DataGeracao)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Contrato>> GetById(int id)
    {
        var contrato = await db.Contratos.AsNoTracking()
            .Include(x => x.Empresa)
            .Include(x => x.PlanoCadastro)
            .Include(x => x.Cobrancas)
            .FirstOrDefaultAsync(x => x.Id == id);
        return contrato is null ? NotFound() : contrato;
    }

    [HttpPost]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<ActionResult<Contrato>> Create(Contrato contrato)
    {
        NormalizarDatas(contrato);
        await AplicarPlano(contrato);
        db.Contratos.Add(contrato);
        db.Cobrancas.Add(GerarCobranca(contrato));
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = contrato.Id }, contrato);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
    public async Task<IActionResult> Update(int id, Contrato contrato)
    {
        var current = await db.Contratos.FindAsync(id);
        if (current is null) return NotFound();

        NormalizarDatas(contrato);
        await AplicarPlano(contrato);
        db.Entry(current).CurrentValues.SetValues(contrato);
        current.Id = id;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.SistemaAdmin)]
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

    private static void NormalizarDatas(Cobranca cobranca)
    {
        cobranca.DataGeracao = DateTime.SpecifyKind(cobranca.DataGeracao, DateTimeKind.Utc);
        cobranca.DataVencimento = DateTime.SpecifyKind(cobranca.DataVencimento, DateTimeKind.Utc);
    }

    private async Task AplicarPlano(Contrato contrato)
    {
        if (contrato.PlanoId is null) return;

        var plano = await db.Planos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == contrato.PlanoId.Value);
        if (plano is null) return;

        contrato.Plano = plano.Nome;
        contrato.LimiteColaboradores = plano.LimiteColaboradores;
        contrato.ValorMensal = plano.ValorCobranca;
        contrato.DataFim ??= contrato.DataInicio.AddDays(plano.PrazoDias);
    }

    private static Cobranca GerarCobranca(Contrato contrato)
    {
        return new Cobranca
        {
            EmpresaId = contrato.EmpresaId,
            Contrato = contrato,
            Descricao = $"Contratacao do plano {contrato.Plano}",
            Valor = contrato.ValorMensal,
            DataGeracao = DateTime.UtcNow,
            DataVencimento = contrato.DataFim ?? contrato.DataInicio.AddDays(30),
            Status = "Pendente"
        };
    }
}
