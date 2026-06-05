using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.DTOs.Curriculos;
using UniFlowPeople.Api.Models;
using UniFlowPeople.Api.Services.Auth;
using UniFlowPeople.Api.Services.Tenancy;

namespace UniFlowPeople.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.EmpresaRoles)]
public class CurriculosController(
    AppDbContext db,
    ITenantContext tenant,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Curriculo>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();
        return await db.Curriculos
            .AsNoTracking()
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.DataCadastro)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<Curriculo>> Create([FromForm] CurriculoUploadRequest request)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var curriculo = new Curriculo
        {
            EmpresaId = tenant.EmpresaId.Value,
            Nome = request.Nome,
            Telefone = request.Telefone,
            Email = request.Email
        };
        if (!string.IsNullOrWhiteSpace(request.Status)) curriculo.Status = request.Status;

        if (request.Arquivo is not null && request.Arquivo.Length > 0)
        {
            var uploadsDir = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "curriculos");
            Directory.CreateDirectory(uploadsDir);

            var extension = Path.GetExtension(request.Arquivo.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await request.Arquivo.CopyToAsync(stream);

            curriculo.NomeArquivo = request.Arquivo.FileName;
            curriculo.CurriculoUrl = $"{Request.Scheme}://{Request.Host}/uploads/curriculos/{fileName}";
        }

        db.Curriculos.Add(curriculo);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = curriculo.Id }, curriculo);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Update(int id, [FromForm] CurriculoUploadRequest request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var curriculo = await db.Curriculos.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (curriculo is null) return NotFound();

        curriculo.Nome = request.Nome;
        curriculo.Telefone = request.Telefone;
        curriculo.Email = request.Email;
        if (!string.IsNullOrWhiteSpace(request.Status)) curriculo.Status = request.Status;

        if (request.Arquivo is not null && request.Arquivo.Length > 0)
        {
            var uploadsDir = Path.Combine(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"), "uploads", "curriculos");
            Directory.CreateDirectory(uploadsDir);

            var extension = Path.GetExtension(request.Arquivo.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await request.Arquivo.CopyToAsync(stream);

            curriculo.NomeArquivo = request.Arquivo.FileName;
            curriculo.CurriculoUrl = $"{Request.Scheme}://{Request.Host}/uploads/curriculos/{fileName}";
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var curriculo = await db.Curriculos.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (curriculo is null) return NotFound();

        db.Curriculos.Remove(curriculo);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
