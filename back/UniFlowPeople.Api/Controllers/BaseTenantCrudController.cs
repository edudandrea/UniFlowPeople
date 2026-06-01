using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.Services.Auth;
using UniFlowPeople.Api.Services.Tenancy;

namespace UniFlowPeople.Api.Controllers;

[Authorize(Roles = Roles.EmpresaRoles)]
public abstract class BaseTenantCrudController<TEntity>(
    AppDbContext db,
    ITenantContext tenant) : ControllerBase
    where TEntity : class
{
    protected abstract DbSet<TEntity> Set { get; }

    [HttpGet]
    public virtual async Task<ActionResult<IEnumerable<TEntity>>> GetAll()
    {
        if (tenant.EmpresaId is null) return Forbid();

        return await Set
            .AsNoTracking()
            .Where(x => EF.Property<int>(x, "EmpresaId") == tenant.EmpresaId.Value)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<TEntity>> GetById(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var item = await Set
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                EF.Property<int>(x, "Id") == id &&
                EF.Property<int>(x, "EmpresaId") == tenant.EmpresaId.Value);

        return item is null ? NotFound() : item;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrRh)]
    public virtual async Task<ActionResult<TEntity>> Create(TEntity entity)
    {
        if (tenant.EmpresaId is null) return Forbid();

        SetEmpresaId(entity, tenant.EmpresaId.Value);
        Set.Add(entity);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = GetId(entity) }, entity);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public virtual async Task<IActionResult> Update(int id, TEntity entity)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var current = await Set.FirstOrDefaultAsync(x =>
            EF.Property<int>(x, "Id") == id &&
            EF.Property<int>(x, "EmpresaId") == tenant.EmpresaId.Value);

        if (current is null) return NotFound();

        db.Entry(current).CurrentValues.SetValues(entity);
        SetId(current, id);
        SetEmpresaId(current, tenant.EmpresaId.Value);
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdminOrRh)]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (tenant.EmpresaId is null) return Forbid();

        var current = await Set.FirstOrDefaultAsync(x =>
            EF.Property<int>(x, "Id") == id &&
            EF.Property<int>(x, "EmpresaId") == tenant.EmpresaId.Value);

        if (current is null) return NotFound();

        Set.Remove(current);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static int GetId(TEntity entity) =>
        (int)(typeof(TEntity).GetProperty("Id")?.GetValue(entity) ?? 0);

    private static void SetId(TEntity entity, int id) =>
        typeof(TEntity).GetProperty("Id")?.SetValue(entity, id);

    private static void SetEmpresaId(TEntity entity, int empresaId) =>
        typeof(TEntity).GetProperty("EmpresaId")?.SetValue(entity, empresaId);
}
