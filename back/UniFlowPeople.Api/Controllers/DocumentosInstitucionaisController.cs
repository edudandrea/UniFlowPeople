using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
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
public class DocumentosInstitucionaisController(
    AppDbContext db,
    ITenantContext tenant,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentoInstitucional>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();

        return await db.DocumentosInstitucionais
            .AsNoTracking()
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.IsModelo)
            .ThenBy(x => x.Titulo)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentoInstitucional>> GetById(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var documento = await db.DocumentosInstitucionais
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);

        return documento is null ? NotFound() : documento;
    }

    [HttpPost("modelos")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<DocumentoInstitucional>> CriarModelo([FromForm] ModeloDocumentoRequest request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Titulo)) return BadRequest("Informe o titulo do modelo.");

        var documento = new DocumentoInstitucional
        {
            EmpresaId = tenant.EmpresaId.Value,
            IsModelo = true,
            TipoDocumento = string.IsNullOrWhiteSpace(request.TipoDocumento) ? "Institucional" : request.TipoDocumento,
            Titulo = request.Titulo.Trim(),
            Conteudo = request.Conteudo ?? string.Empty,
            DocumentoAdmissao = request.DocumentoAdmissao,
            DocumentoDemissao = request.DocumentoDemissao
        };

        await AplicarArquivoModelo(documento, request.Arquivo);
        db.DocumentosInstitucionais.Add(documento);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = documento.Id }, documento);
    }

    [HttpPut("modelos/{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> AtualizarModelo(int id, [FromForm] ModeloDocumentoRequest request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var documento = await db.DocumentosInstitucionais
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value && x.IsModelo);
        if (documento is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Titulo)) return BadRequest("Informe o titulo do modelo.");

        documento.Titulo = request.Titulo.Trim();
        documento.TipoDocumento = string.IsNullOrWhiteSpace(request.TipoDocumento) ? "Institucional" : request.TipoDocumento;
        documento.Conteudo = request.Conteudo ?? string.Empty;
        documento.DocumentoAdmissao = request.DocumentoAdmissao;
        documento.DocumentoDemissao = request.DocumentoDemissao;

        await AplicarArquivoModelo(documento, request.Arquivo);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("modelos/preview-texto")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<PreviewTextoModeloResponse>> PreviewTextoModelo([FromForm] IFormFile arquivo)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (arquivo is null || arquivo.Length == 0) return BadRequest("Selecione um arquivo para extrair o texto.");

        var extension = Path.GetExtension(arquivo.FileName);
        var tempDir = Path.Combine(Path.GetTempPath(), "uniflowpeople-modelos");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, $"{Guid.NewGuid():N}{extension}");

        try
        {
            await using (var stream = System.IO.File.Create(tempPath))
            {
                await arquivo.CopyToAsync(stream);
            }

            var conteudo = await ExtrairTextoSeguro(tempPath, extension);
            return new PreviewTextoModeloResponse(conteudo);
        }
        finally
        {
            if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var documento = await db.DocumentosInstitucionais
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (documento is null) return NotFound();

        RemoverArquivoFisico(documento.UrlArquivoModelo);
        db.DocumentosInstitucionais.Remove(documento);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task AplicarArquivoModelo(DocumentoInstitucional documento, IFormFile? arquivo)
    {
        if (arquivo is null || arquivo.Length == 0) return;

        RemoverArquivoFisico(documento.UrlArquivoModelo);

        var uploadsDir = Path.Combine(WebRoot(), "uploads", "modelos-institucionais");
        Directory.CreateDirectory(uploadsDir);

        var extension = Path.GetExtension(arquivo.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await arquivo.CopyToAsync(stream);
        }

        documento.NomeArquivoModelo = arquivo.FileName;
        documento.TipoArquivoModelo = string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType;
        documento.UrlArquivoModelo = $"{Request.Scheme}://{Request.Host}/uploads/modelos-institucionais/{fileName}";

        var textoExtraido = await ExtrairTextoSeguro(filePath, extension);
        if (!string.IsNullOrWhiteSpace(textoExtraido))
        {
            documento.Conteudo = textoExtraido;
        }
    }

    private static async Task<string> ExtrairTextoSeguro(string filePath, string extension)
    {
        try
        {
            return await ExtrairTexto(filePath, extension);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static async Task<string> ExtrairTexto(string filePath, string extension)
    {
        extension = extension.ToLowerInvariant();
        if (extension == ".txt") return await System.IO.File.ReadAllTextAsync(filePath);
        if (extension == ".docx") return ExtrairDocx(filePath);
        if (extension is ".pdf" or ".doc") return ExtrairTextoMelhorEsforco(await System.IO.File.ReadAllBytesAsync(filePath));
        return string.Empty;
    }

    private static string ExtrairDocx(string filePath)
    {
        using var archive = ZipFile.OpenRead(filePath);
        var entry = archive.GetEntry("word/document.xml");
        if (entry is null) return string.Empty;

        using var stream = entry.Open();
        var document = XDocument.Load(stream);
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        var paragraphs = document.Descendants(w + "p")
            .Select(p => string.Concat(p.Descendants(w + "t").Select(t => t.Value)).Trim())
            .Where(text => !string.IsNullOrWhiteSpace(text));

        return string.Join(Environment.NewLine + Environment.NewLine, paragraphs);
    }

    private static string ExtrairTextoMelhorEsforco(byte[] bytes)
    {
        var raw = Encoding.UTF8.GetString(bytes);
        var latin = Encoding.Latin1.GetString(bytes);
        var matches = Regex.Matches($"{raw} {latin}", @"\(([^\)]{3,})\)|<([0-9A-Fa-f\s]{8,})>|([A-Za-zÀ-ÿ0-9][A-Za-zÀ-ÿ0-9\s.,;:!?ºª°()/+\-]{12,})");
        var parts = matches
            .Select(match =>
            {
                if (match.Groups[1].Success) return match.Groups[1].Value;
                if (match.Groups[2].Success) return HexToText(match.Groups[2].Value);
                return match.Groups[3].Value;
            })
            .Select(text => Regex.Unescape(text).Trim())
            .Where(text => Regex.IsMatch(text, @"[A-Za-zÀ-ÿ]"));

        var extracted = string.Join(" ", parts);
        return Regex.Replace(extracted, @"\s{2,}", " ").Trim();
    }

    private static string HexToText(string hex)
    {
        var clean = Regex.Replace(hex, @"\s+", "");
        if (clean.Length % 2 != 0) return string.Empty;

        try
        {
            var bytes = Enumerable.Range(0, clean.Length / 2)
                .Select(i => Convert.ToByte(clean.Substring(i * 2, 2), 16))
                .ToArray();

            return Encoding.BigEndianUnicode.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }

    private string WebRoot() => environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");

    private void RemoverArquivoFisico(string? urlArquivo)
    {
        if (string.IsNullOrWhiteSpace(urlArquivo) || !Uri.TryCreate(urlArquivo, UriKind.Absolute, out var uri)) return;
        var relativePath = uri.AbsolutePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRoot = WebRoot();
        var filePath = Path.GetFullPath(Path.Combine(webRoot, relativePath));
        if (!filePath.StartsWith(Path.GetFullPath(webRoot), StringComparison.OrdinalIgnoreCase)) return;
        if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
    }

    public sealed class ModeloDocumentoRequest
    {
        public string Titulo { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public string? Conteudo { get; set; }
        public bool DocumentoAdmissao { get; set; }
        public bool DocumentoDemissao { get; set; }
        public IFormFile? Arquivo { get; set; }
    }

    public sealed record PreviewTextoModeloResponse(string Conteudo);
}
