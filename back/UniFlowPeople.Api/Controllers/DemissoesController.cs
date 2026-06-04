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

        var etapasConfig = await EtapasConfig("Demissao");
        foreach (var processo in processos)
        {
            SincronizarEtapasPadrao(processo, etapasConfig);
            AtualizarStatusProcesso(processo, etapasConfig.Select(x => x.Nome).ToList());
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
            entity.Etapas = (await EtapasConfig("Demissao")).Select(etapa => new DemissaoEtapa { Nome = etapa.Nome }).ToList();
        }

        AtualizarStatusProcesso(entity, (await EtapasConfig("Demissao")).Select(x => x.Nome).ToList());
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
            var etapasConfig = await EtapasConfig("Demissao");
            SincronizarEtapasPadrao(processo, etapasConfig);
            AtualizarStatusProcesso(processo, etapasConfig.Select(x => x.Nome).ToList());
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
                var nomes = (await EtapasConfig("Demissao")).Select(x => x.Nome).ToList();
                var indice = nomes.FindIndex(nome => nome == etapa.Nome);
                foreach (var posterior in processo.Etapas.Where(x => nomes.IndexOf(x.Nome) > indice))
                {
                    posterior.Status = "Pendente";
                    posterior.DataConclusao = null;
                }
            }

            AtualizarStatusProcesso(processo, (await EtapasConfig("Demissao")).Select(x => x.Nome).ToList());
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
        var etapasConfig = await EtapasConfig("Demissao");
        SincronizarEtapasPadrao(processo, etapasConfig);

        if (etapasConfig.Any(etapa => processo.Etapas.FirstOrDefault(x => x.Nome == etapa.Nome)?.Status != "Concluida"))
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

    private async Task<List<EtapaProcessoConfig>> EtapasConfig(string tipoProcesso)
    {
        if (tenant.EmpresaId is null) return EtapasPadrao();
        var etapas = await db.EtapasProcessosConfig
            .AsNoTracking()
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value && x.TipoProcesso == tipoProcesso && x.Ativa)
            .OrderBy(x => x.Ordem)
            .ThenBy(x => x.Id)
            .ToListAsync();

        return etapas.Count > 0 ? etapas : EtapasPadrao();
    }

    private static List<EtapaProcessoConfig> EtapasPadrao() =>
    [
        new() { Nome = "Aprovada pela direcao", Ordem = 1 },
        new() { Nome = "Entrevista demissional", Ordem = 2 },
        new() { Nome = "Exame demissional", Ordem = 3 },
        new() { Nome = "Demissao efetivada", Ordem = 4 }
    ];

    private static void SincronizarEtapasPadrao(DemissaoProcesso processo, IReadOnlyList<EtapaProcessoConfig> etapasConfig)
    {
        var existentes = processo.Etapas.Select(x => x.Nome).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var etapa in etapasConfig.Where(etapa => !existentes.Contains(etapa.Nome)))
        {
            processo.Etapas.Add(new DemissaoEtapa
            {
                DemissaoProcessoId = processo.Id,
                Nome = etapa.Nome,
                Status = "Pendente"
            });
        }
    }

    private static void AtualizarStatusProcesso(DemissaoProcesso processo, IReadOnlyList<string> nomesEtapas)
    {
        if (processo.Status is "Concluido" or "Cancelado") return;

        var etapaAtual = nomesEtapas
            .Select(nome => processo.Etapas.FirstOrDefault(x => x.Nome == nome))
            .FirstOrDefault(etapa => etapa is not null && etapa.Status != "Concluida");

        processo.Status = etapaAtual?.Nome ?? "Pronto para efetivar";
    }
}
