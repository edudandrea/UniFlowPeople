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
public class AdmissoesController(
    AppDbContext db,
    ITenantContext tenant,
    IWebHostEnvironment environment) : BaseTenantCrudController<AdmissaoProcesso>(db, tenant)
{
    protected override DbSet<AdmissaoProcesso> Set => db.Admissoes;

    [HttpGet]
    public override async Task<ActionResult<IEnumerable<AdmissaoProcesso>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();

        var processos = await db.Admissoes
            .Include(x => x.Etapas)
            .Include(x => x.DocumentosAnexados)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.DataCadastro)
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
    public override async Task<ActionResult<AdmissaoProcesso>> Create(AdmissaoProcesso entity)
    {
        if (tenant.EmpresaId is null) return Forbid();

        entity.EmpresaId = tenant.EmpresaId.Value;
        if (!entity.Etapas.Any())
        {
            entity.Etapas = EtapasPadrao()
                .Select((nome, index) => new AdmissaoEtapa
                {
                    Nome = nome,
                    Status = index == 0 ? "Concluida" : "Pendente",
                    DataConclusao = index == 0 ? DateTime.UtcNow : null
                })
                .ToList();
        }
        AtualizarStatusProcesso(entity);

        db.Admissoes.Add(entity);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpGet("{id:int}/detalhes")]
    public async Task<ActionResult<AdmissaoProcesso>> Detalhes(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Admissoes
            .Include(x => x.Etapas)
            .Include(x => x.DocumentosAnexados)
            .Include(x => x.DocumentosInstitucionais)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);

        if (processo is not null)
        {
            SincronizarEtapasPadrao(processo);
            AtualizarStatusProcesso(processo);
            await db.SaveChangesAsync();
        }

        return processo is null ? NotFound() : processo;
    }

    [HttpPut("{id:int}/etapas/{etapaId:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> AtualizarEtapa(int id, int etapaId, AdmissaoEtapa request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var etapa = await db.AdmissaoEtapas
            .Include(x => x.AdmissaoProcesso)
            .ThenInclude(x => x.Etapas)
            .FirstOrDefaultAsync(x => x.Id == etapaId && x.AdmissaoProcessoId == id && x.AdmissaoProcesso.EmpresaId == tenant.EmpresaId.Value);
        if (etapa is null) return NotFound();
        if (etapa.AdmissaoProcesso.Status is "Admitido" or "Cancelado") return BadRequest("Nao e possivel alterar etapas de um processo finalizado.");

        etapa.Status = request.Status;
        etapa.Responsavel = request.Responsavel;
        etapa.Observacoes = request.Observacoes;
        etapa.DataConclusao = request.Status == "Concluida" ? request.DataConclusao ?? DateTime.UtcNow : null;
        if (request.Status != "Concluida")
        {
            var nomes = EtapasPadrao().ToList();
            var etapaIndex = nomes.IndexOf(etapa.Nome);
            foreach (var etapaPosterior in etapa.AdmissaoProcesso.Etapas.Where(x => nomes.IndexOf(x.Nome) > etapaIndex))
            {
                etapaPosterior.Status = "Pendente";
                etapaPosterior.DataConclusao = null;
            }
        }
        AtualizarStatusProcesso(etapa.AdmissaoProcesso);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/admitir")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<AdmissaoProcesso>> Admitir(int id, AdmitirAdmissaoRequest? request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Admissoes
            .Include(x => x.Etapas)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (processo is null) return NotFound();
        if (processo.Status == "Cancelado") return BadRequest("Processo cancelado nao pode ser admitido.");
        SincronizarEtapasPadrao(processo);
        if (EtapasPadrao().Any(nome => processo.Etapas.FirstOrDefault(x => x.Nome == nome)?.Status != "Concluida"))
        {
            return BadRequest("Todas as etapas precisam estar concluidas antes da admissao.");
        }

        if (request?.ColaboradorId is not null)
        {
            var colaboradorExiste = await db.Colaboradores.AnyAsync(x => x.Id == request.ColaboradorId && x.EmpresaId == tenant.EmpresaId.Value);
            if (!colaboradorExiste) return BadRequest("Colaborador nao encontrado para vincular a admissao.");
            processo.ColaboradorId = request.ColaboradorId;
        }

        processo.Status = "Admitido";
        await db.SaveChangesAsync();
        return processo;
    }

    [HttpPost("{id:int}/cancelar")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Cancelar(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Admissoes.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (processo is null) return NotFound();
        if (processo.Status == "Admitido") return BadRequest("Processo admitido nao pode ser cancelado.");

        processo.Status = "Cancelado";
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/documentos-anexados")]
    public async Task<ActionResult<IEnumerable<AdmissaoDocumento>>> DocumentosAnexados(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processoExiste = await db.Admissoes.AnyAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (!processoExiste) return NotFound();

        return await db.AdmissaoDocumentos
            .AsNoTracking()
            .Where(x => x.AdmissaoProcessoId == id && x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.DataUpload)
            .ToListAsync();
    }

    [HttpPost("{id:int}/documentos-anexados")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<AdmissaoDocumento>> AnexarDocumento(int id, [FromForm] IFormFile arquivo)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (arquivo is null || arquivo.Length == 0) return BadRequest("Selecione um arquivo para anexar.");

        var processo = await db.Admissoes.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (processo is null) return NotFound();
        if (processo.Status is "Admitido" or "Cancelado") return BadRequest("Nao e possivel anexar documentos a um processo finalizado.");

        var uploadsDir = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "admissoes", id.ToString());
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(arquivo.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await arquivo.CopyToAsync(stream);
        }

        var documento = new AdmissaoDocumento
        {
            EmpresaId = tenant.EmpresaId.Value,
            AdmissaoProcessoId = id,
            NomeArquivo = arquivo.FileName,
            TipoArquivo = string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType,
            UrlArquivo = $"{Request.Scheme}://{Request.Host}/uploads/admissoes/{id}/{fileName}"
        };

        db.AdmissaoDocumentos.Add(documento);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(DocumentosAnexados), new { id }, documento);
    }

    [HttpDelete("{id:int}/documentos-anexados/{documentoId:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> RemoverDocumentoAnexado(int id, int documentoId)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var documento = await db.AdmissaoDocumentos
            .FirstOrDefaultAsync(x => x.Id == documentoId && x.AdmissaoProcessoId == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (documento is null) return NotFound();

        RemoverArquivoFisico(documento.UrlArquivo);
        db.AdmissaoDocumentos.Remove(documento);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/documentos")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<IEnumerable<DocumentoInstitucional>>> GerarDocumentosInstitucionais(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var processo = await db.Admissoes
            .Include(x => x.DocumentosInstitucionais)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (processo is null) return NotFound();
        if (processo.Status != "Admitido") return BadRequest("Conclua a admissao antes de gerar a documentacao institucional.");
        if (processo.DocumentosInstitucionais.Any()) return processo.DocumentosInstitucionais.ToList();

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

    private void RemoverArquivoFisico(string urlArquivo)
    {
        if (!Uri.TryCreate(urlArquivo, UriKind.Absolute, out var uri)) return;
        var relativePath = uri.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var filePath = Path.GetFullPath(Path.Combine(webRoot, relativePath));
        if (!filePath.StartsWith(Path.GetFullPath(webRoot), StringComparison.OrdinalIgnoreCase)) return;
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
    }

    private static string[] EtapasPadrao() =>
    [
        "Candidato cadastrado",
        "Entrevista com RH",
        "Entrevista com gestor",
        "Avaliacao psicologica",
        "Anexar documentos"
    ];

    private static void SincronizarEtapasPadrao(AdmissaoProcesso processo)
    {
        var existentes = processo.Etapas.Select(x => x.Nome).ToHashSet();
        foreach (var nome in EtapasPadrao().Where(nome => !existentes.Contains(nome)))
        {
            var primeiraEtapa = nome == "Candidato cadastrado";
            processo.Etapas.Add(new AdmissaoEtapa
            {
                Nome = nome,
                Status = primeiraEtapa ? "Concluida" : "Pendente",
                DataConclusao = primeiraEtapa ? DateTime.UtcNow : null
            });
        }
    }

    private static void AtualizarStatusProcesso(AdmissaoProcesso processo)
    {
        if (processo.Status is "Admitido" or "Cancelado") return;

        var etapaAtual = EtapasPadrao()
            .Select(nome => processo.Etapas.FirstOrDefault(x => x.Nome == nome))
            .FirstOrDefault(etapa => etapa?.Status != "Concluida");

        processo.Status = etapaAtual?.Nome ?? "Pronto para admissao";
    }

    public sealed class AdmitirAdmissaoRequest
    {
        public int? ColaboradorId { get; set; }
    }
}
