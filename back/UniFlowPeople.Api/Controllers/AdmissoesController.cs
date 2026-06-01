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
public class AdmissoesController(AppDbContext db, ITenantContext tenant) : BaseTenantCrudController<AdmissaoProcesso>(db, tenant)
{
    protected override DbSet<AdmissaoProcesso> Set => db.Admissoes;

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public override async Task<ActionResult<AdmissaoProcesso>> Create(AdmissaoProcesso entity)
    {
        if (tenant.EmpresaId is null) return Forbid();

        entity.EmpresaId = tenant.EmpresaId.Value;
        if (!entity.Etapas.Any())
        {
            entity.Etapas = EtapasPadrao().Select(nome => new AdmissaoEtapa { Nome = nome }).ToList();
        }

        db.Admissoes.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpGet("{id:int}/detalhes")]
    public async Task<ActionResult<AdmissaoProcesso>> Detalhes(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Admissoes
            .AsNoTracking()
            .Include(x => x.Etapas)
            .Include(x => x.DocumentosInstitucionais)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);

        return processo is null ? NotFound() : processo;
    }

    [HttpPut("{id:int}/etapas/{etapaId:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> AtualizarEtapa(int id, int etapaId, AdmissaoEtapa request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var etapa = await db.AdmissaoEtapas
            .Include(x => x.AdmissaoProcesso)
            .FirstOrDefaultAsync(x => x.Id == etapaId && x.AdmissaoProcessoId == id && x.AdmissaoProcesso.EmpresaId == tenant.EmpresaId.Value);
        if (etapa is null) return NotFound();

        etapa.Status = request.Status;
        etapa.Responsavel = request.Responsavel;
        etapa.Observacoes = request.Observacoes;
        etapa.DataConclusao = request.Status == "Concluida" ? DateTime.UtcNow : null;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/admitir")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<IEnumerable<DocumentoInstitucional>>> Admitir(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Admissoes
            .Include(x => x.DocumentosInstitucionais)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (processo is null) return NotFound();

        processo.Status = "Admitido";
        var documentos = GerarDocumentos(processo);
        foreach (var documento in documentos)
        {
            db.DocumentosInstitucionais.Add(documento);
        }

        await db.SaveChangesAsync();
        return documentos;
    }

    private DocumentoInstitucional[] GerarDocumentos(AdmissaoProcesso processo) =>
    [
        NovoDocumento(processo, "Termo de admissao", $"Termo de admissao de {processo.NomeCandidato} para o cargo {processo.CargoPretendido}."),
        NovoDocumento(processo, "Politica institucional", $"Declaro ciencia das politicas internas da empresa na data de admissao prevista {processo.DataPrevistaAdmissao:dd/MM/yyyy}."),
        NovoDocumento(processo, "Checklist de onboarding", $"Checklist institucional gerado para {processo.NomeCandidato}.")
    ];

    private DocumentoInstitucional NovoDocumento(AdmissaoProcesso processo, string tipo, string conteudo) => new()
    {
        EmpresaId = processo.EmpresaId,
        ColaboradorId = processo.ColaboradorId,
        AdmissaoProcessoId = processo.Id,
        TipoDocumento = tipo,
        Titulo = tipo,
        Conteudo = conteudo
    };

    private static string[] EtapasPadrao() =>
    [
        "Dados pessoais",
        "Documentos obrigatorios",
        "Exame admissional",
        "Contrato e beneficios",
        "Acessos internos",
        "Onboarding"
    ];
}
