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
public class SolicitacoesController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SolicitacaoRh>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();

        var query = db.SolicitacoesRh
            .AsNoTracking()
            .Include(x => x.Colaborador)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value);

        if (tenant.Role == Roles.Colaborador)
        {
            query = query.Where(x =>
                (tenant.ColaboradorId != null && x.ColaboradorId == tenant.ColaboradorId) ||
                (tenant.UsuarioId != null && x.SolicitanteUsuarioId == tenant.UsuarioId));
        }

        return await query
            .OrderByDescending(x => x.DataSolicitacao)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SolicitacaoRh>> GetById(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var item = await db.SolicitacoesRh
            .AsNoTracking()
            .Include(x => x.Colaborador)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);

        if (item is null) return NotFound();
        if (!PodeAcessarSolicitacao(item)) return Forbid();

        return item;
    }

    [HttpPost]
    public async Task<ActionResult<SolicitacaoRh>> Create(SolicitacaoRh solicitacao)
    {
        if (tenant.EmpresaId is null) return Forbid();

        solicitacao.Id = 0;
        solicitacao.EmpresaId = tenant.EmpresaId.Value;
        solicitacao.SolicitanteUsuarioId = tenant.UsuarioId;
        solicitacao.Status = string.IsNullOrWhiteSpace(solicitacao.Status) ? "Enviada" : solicitacao.Status;
        solicitacao.DataSolicitacao = DateTime.UtcNow;
        solicitacao.DataAtualizacao = null;
        solicitacao.DataConclusao = null;

        if (tenant.Role == Roles.Colaborador)
        {
            solicitacao.ColaboradorId = tenant.ColaboradorId;
            solicitacao.Status = "Enviada";
            solicitacao.RespostaRh = null;
        }

        db.SolicitacoesRh.Add(solicitacao);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = solicitacao.Id }, solicitacao);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Update(int id, SolicitacaoRh solicitacao)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var current = await db.SolicitacoesRh
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);

        if (current is null) return NotFound();

        current.ColaboradorId = solicitacao.ColaboradorId;
        current.Tipo = solicitacao.Tipo;
        current.Titulo = solicitacao.Titulo;
        current.Descricao = solicitacao.Descricao;
        current.Prioridade = solicitacao.Prioridade;
        current.Status = solicitacao.Status;
        current.RespostaRh = solicitacao.RespostaRh;
        current.DataAtualizacao = DateTime.UtcNow;
        current.DataConclusao = solicitacao.Status == "Concluida" ? DateTime.UtcNow : null;

        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var current = await db.SolicitacoesRh
            .FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);

        if (current is null) return NotFound();

        db.SolicitacoesRh.Remove(current);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private bool PodeAcessarSolicitacao(SolicitacaoRh item)
    {
        if (tenant.Role != Roles.Colaborador) return true;

        return (tenant.ColaboradorId != null && item.ColaboradorId == tenant.ColaboradorId) ||
            (tenant.UsuarioId != null && item.SolicitanteUsuarioId == tenant.UsuarioId);
    }
}
