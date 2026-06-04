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
public class CargosEpisController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CargoEpi>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();

        return await db.CargosEpis
            .AsNoTracking()
            .Include(x => x.Cargo)
            .Include(x => x.Epi)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderBy(x => x.Cargo.Nome)
            .ThenBy(x => x.Epi.Nome)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<CargoEpi>> Create(CargoEpi vinculo)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (!await CargoPertenceEmpresa(vinculo.CargoId) || !await EpiPertenceEmpresa(vinculo.EpiId)) return Forbid();

        vinculo.EmpresaId = tenant.EmpresaId.Value;
        db.CargosEpis.Add(vinculo);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = vinculo.Id }, vinculo);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Update(int id, CargoEpi request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (!await CargoPertenceEmpresa(request.CargoId) || !await EpiPertenceEmpresa(request.EpiId)) return Forbid();

        var vinculo = await db.CargosEpis.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (vinculo is null) return NotFound();

        vinculo.CargoId = request.CargoId;
        vinculo.EpiId = request.EpiId;
        vinculo.QuantidadePadrao = request.QuantidadePadrao;
        vinculo.Obrigatorio = request.Obrigatorio;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var vinculo = await db.CargosEpis.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (vinculo is null) return NotFound();

        db.CargosEpis.Remove(vinculo);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> CargoPertenceEmpresa(int cargoId) =>
        tenant.EmpresaId.HasValue && await db.Cargos.AnyAsync(x => x.Id == cargoId && x.EmpresaId == tenant.EmpresaId.Value);

    private async Task<bool> EpiPertenceEmpresa(int epiId) =>
        tenant.EmpresaId.HasValue && await db.Epis.AnyAsync(x => x.Id == epiId && x.EmpresaId == tenant.EmpresaId.Value);
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.EmpresaRoles)]
public class ColaboradoresEpisController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ColaboradorEpi>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();

        return await db.ColaboradoresEpis
            .AsNoTracking()
            .Include(x => x.Colaborador)
            .Include(x => x.Epi)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.DataRetirada)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<ColaboradorEpi>> Create(ColaboradorEpi retirada)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (!await ColaboradorPertenceEmpresa(retirada.ColaboradorId) || !await EpiPertenceEmpresa(retirada.EpiId)) return Forbid();

        retirada.EmpresaId = tenant.EmpresaId.Value;
        await PreencherTrocaPrevista(retirada);
        db.ColaboradoresEpis.Add(retirada);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = retirada.Id }, retirada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Update(int id, ColaboradorEpi request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (!await ColaboradorPertenceEmpresa(request.ColaboradorId) || !await EpiPertenceEmpresa(request.EpiId)) return Forbid();

        var retirada = await db.ColaboradoresEpis.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (retirada is null) return NotFound();

        retirada.ColaboradorId = request.ColaboradorId;
        retirada.EpiId = request.EpiId;
        retirada.Quantidade = request.Quantidade;
        retirada.DataRetirada = request.DataRetirada;
        retirada.DataPrevistaTroca = request.DataPrevistaTroca;
        retirada.DataDevolucao = request.DataDevolucao;
        retirada.Status = request.Status;
        retirada.Observacoes = request.Observacoes;
        await PreencherTrocaPrevista(retirada);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var retirada = await db.ColaboradoresEpis.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (retirada is null) return NotFound();

        db.ColaboradoresEpis.Remove(retirada);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task PreencherTrocaPrevista(ColaboradorEpi retirada)
    {
        if (retirada.DataPrevistaTroca.HasValue) return;

        var dias = await db.Epis
            .Where(x => x.Id == retirada.EpiId)
            .Select(x => x.PeriodicidadeTrocaDias)
            .FirstOrDefaultAsync();

        if (dias > 0) retirada.DataPrevistaTroca = retirada.DataRetirada.AddDays(dias);
    }

    private async Task<bool> ColaboradorPertenceEmpresa(int colaboradorId) =>
        tenant.EmpresaId.HasValue && await db.Colaboradores.AnyAsync(x => x.Id == colaboradorId && x.EmpresaId == tenant.EmpresaId.Value);

    private async Task<bool> EpiPertenceEmpresa(int epiId) =>
        tenant.EmpresaId.HasValue && await db.Epis.AnyAsync(x => x.Id == epiId && x.EmpresaId == tenant.EmpresaId.Value);
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.EmpresaRoles)]
public class ColaboradoresFerramentasAcessoController(AppDbContext db, ITenantContext tenant) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ColaboradorFerramentaAcesso>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();

        return await db.ColaboradoresFerramentasAcesso
            .AsNoTracking()
            .Include(x => x.Colaborador)
            .Include(x => x.FerramentaAcesso)
            .Where(x => x.EmpresaId == tenant.EmpresaId.Value)
            .OrderByDescending(x => x.DataEntrega)
            .ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<ActionResult<ColaboradorFerramentaAcesso>> Create(ColaboradorFerramentaAcesso vinculo)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (!await ColaboradorPertenceEmpresa(vinculo.ColaboradorId) || !await FerramentaPertenceEmpresa(vinculo.FerramentaAcessoId))
            return Forbid();

        vinculo.EmpresaId = tenant.EmpresaId.Value;
        db.ColaboradoresFerramentasAcesso.Add(vinculo);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = vinculo.Id }, vinculo);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Update(int id, ColaboradorFerramentaAcesso request)
    {
        if (tenant.EmpresaId is null) return Forbid();
        if (!await ColaboradorPertenceEmpresa(request.ColaboradorId) || !await FerramentaPertenceEmpresa(request.FerramentaAcessoId))
            return Forbid();

        var vinculo = await db.ColaboradoresFerramentasAcesso.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (vinculo is null) return NotFound();

        vinculo.ColaboradorId = request.ColaboradorId;
        vinculo.FerramentaAcessoId = request.FerramentaAcessoId;
        vinculo.DataEntrega = request.DataEntrega;
        vinculo.DataDevolucao = request.DataDevolucao;
        vinculo.Status = request.Status;
        vinculo.Observacoes = request.Observacoes;
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var vinculo = await db.ColaboradoresFerramentasAcesso.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (vinculo is null) return NotFound();

        db.ColaboradoresFerramentasAcesso.Remove(vinculo);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<bool> ColaboradorPertenceEmpresa(int colaboradorId) =>
        tenant.EmpresaId.HasValue && await db.Colaboradores.AnyAsync(x => x.Id == colaboradorId && x.EmpresaId == tenant.EmpresaId.Value);

    private async Task<bool> FerramentaPertenceEmpresa(int ferramentaId) =>
        tenant.EmpresaId.HasValue && await db.FerramentasAcesso.AnyAsync(x => x.Id == ferramentaId && x.EmpresaId == tenant.EmpresaId.Value);
}
