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
public class TreinamentosController(AppDbContext db, ITenantContext tenant) : BaseTenantCrudController<Treinamento>(db, tenant)
{
    protected override DbSet<Treinamento> Set => db.Treinamentos;

    [HttpGet("{id:int}/participantes")]
    public async Task<ActionResult<IEnumerable<TreinamentoColaborador>>> Participantes(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();
        var pertenceEmpresa = await db.Treinamentos.AnyAsync(x => x.Id == id && x.EmpresaId == tenant.EmpresaId.Value);
        if (!pertenceEmpresa) return NotFound();

        return await db.TreinamentosColaboradores
            .AsNoTracking()
            .Include(x => x.Colaborador)
            .Where(x => x.TreinamentoId == id)
            .OrderBy(x => x.Colaborador.Nome)
            .ToListAsync();
    }
}
