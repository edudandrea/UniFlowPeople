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
public class DemissoesController(AppDbContext db, ITenantContext tenant) : BaseTenantCrudController<DemissaoProcesso>(db, tenant)
{
    protected override DbSet<DemissaoProcesso> Set => db.Demissoes;

    [HttpGet]
    public override async Task<ActionResult<IEnumerable<DemissaoProcesso>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processos = await db.Demissoes
            .Include(x => x.Colaborador)
            .Include(x => x.Etapas)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.DataSolicitacao)
            .ToListAsync();

        foreach (var processo in processos)
        {
            SincronizarEtapasPadrao(processo);
            AtualizarStatusProcesso(processo);
        }

        await db.SaveChangesAsync();
        return processos;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public override async Task<ActionResult<DemissaoProcesso>> Create(DemissaoProcesso entity)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var colaboradorOk = await db.Colaboradores.AnyAsync(x => x.Id == entity.ColaboradorId && x.EmpresaId == tenant.EmpresaId.Value);
        if (!colaboradorOk) return Forbid();

        entity.EmpresaId = tenant.EmpresaId.Value;
        if (!entity.Etapas.Any())
        {
            entity.Etapas = EtapasPadrao().Select(nome => new DemissaoEtapa { Nome = nome }).ToList();
        }

        AtualizarStatusProcesso(entity);
        db.Demissoes.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpGet("{id:int}/detalhes")]
    public async Task<ActionResult<DemissaoProcesso>> Detalhes(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Demissoes
            .AsNoTracking()
            .Include(x => x.Colaborador)
            .Include(x => x.Etapas)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);

        if (processo is not null)
        {
            SincronizarEtapasPadrao(processo);
            AtualizarStatusProcesso(processo);
        }

        return processo is null ? NotFound() : processo;
    }

    [HttpPut("{id:int}/etapas/{etapaId:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> AtualizarEtapa(int id, int etapaId, DemissaoEtapa request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var etapa = await db.DemissaoEtapas
            .Include(x => x.DemissaoProcesso)
            .FirstOrDefaultAsync(x => x.Id == etapaId && x.DemissaoProcessoId == id && x.DemissaoProcesso.EmpresaId == tenant.EmpresaId.Value);
        if (etapa is null) return NotFound();

        etapa.Status = request.Status;
        etapa.Responsavel = request.Responsavel;
        etapa.Observacoes = request.Observacoes;
        etapa.DataConclusao = request.Status == "Concluida" ? request.DataConclusao ?? DateTime.UtcNow : null;

        var processo = await db.Demissoes
            .Include(x => x.Etapas)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (processo is not null)
        {
            if (request.Status != "Concluida")
            {
                var nomes = EtapasPadrao().ToList();
                var indice = nomes.FindIndex(nome => nome == etapa.Nome);
                foreach (var posterior in processo.Etapas.Where(x => nomes.IndexOf(x.Nome) > indice))
                {
                    posterior.Status = "Pendente";
                    posterior.DataConclusao = null;
                }
            }

            AtualizarStatusProcesso(processo);
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/concluir")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Concluir(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Demissoes
            .Include(x => x.Colaborador)
            .Include(x => x.Etapas)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (processo is null) return NotFound();
        SincronizarEtapasPadrao(processo);

        if (EtapasPadrao().Any(nome => processo.Etapas.FirstOrDefault(x => x.Nome == nome)?.Status != "Concluida"))
        {
            return BadRequest("Conclua todas as etapas antes de efetivar a demissao.");
        }

        processo.Status = "Concluido";
        processo.Colaborador.Status = "Demitido";
        processo.Colaborador.Ativo = false;
        processo.Colaborador.DataDemissao = processo.DataDesligamento ?? DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static string[] EtapasPadrao() =>
    [
        "Aprovada pela direcao",
        "Entrevista demissional",
        "Exame demissional",
        "Demissao efetivada"
    ];

    private static void SincronizarEtapasPadrao(DemissaoProcesso processo)
    {
        var existentes = processo.Etapas.Select(x => x.Nome).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var nome in EtapasPadrao().Where(nome => !existentes.Contains(nome)))
        {
            processo.Etapas.Add(new DemissaoEtapa
            {
                DemissaoProcessoId = processo.Id,
                Nome = nome,
                Status = "Pendente"
            });
        }
    }

    private static void AtualizarStatusProcesso(DemissaoProcesso processo)
    {
        if (processo.Status is "Concluido" or "Cancelado") return;

        var etapaAtual = EtapasPadrao()
            .Select(nome => processo.Etapas.FirstOrDefault(x => x.Nome == nome))
            .FirstOrDefault(etapa => etapa is not null && etapa.Status != "Concluida");

        processo.Status = etapaAtual?.Nome ?? "Pronto para efetivar";
    }
}
