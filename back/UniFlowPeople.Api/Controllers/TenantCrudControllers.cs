using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.Models;
using UniFlowPeople.Api.Services.Tenancy;

namespace UniFlowPeople.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FiliaisController : BaseTenantCrudController<Filial>
{
    private readonly AppDbContext _db;
    public FiliaisController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<Filial> Set => _db.Filiais;
}

[ApiController]
[Route("api/[controller]")]
public class DepartamentosController : BaseTenantCrudController<Departamento>
{
    private readonly AppDbContext _db;
    public DepartamentosController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<Departamento> Set => _db.Departamentos;
}

[ApiController]
[Route("api/[controller]")]
public class CargosController : BaseTenantCrudController<Cargo>
{
    private readonly AppDbContext _db;
    public CargosController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<Cargo> Set => _db.Cargos;
}

[ApiController]
[Route("api/[controller]")]
public class ColaboradoresController : BaseTenantCrudController<Colaborador>
{
    private readonly AppDbContext _db;
    public ColaboradoresController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<Colaborador> Set => _db.Colaboradores;
}

[ApiController]
[Route("api/[controller]")]
public class BeneficiosController : BaseTenantCrudController<Beneficio>
{
    private readonly AppDbContext _db;
    public BeneficiosController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<Beneficio> Set => _db.Beneficios;
}

[ApiController]
[Route("api/[controller]")]
public class EpisController : BaseTenantCrudController<Epi>
{
    private readonly AppDbContext _db;
    public EpisController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<Epi> Set => _db.Epis;
}

[ApiController]
[Route("api/[controller]")]
public class FerramentasAcessoController : BaseTenantCrudController<FerramentaAcesso>
{
    private readonly AppDbContext _db;
    public FerramentasAcessoController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<FerramentaAcesso> Set => _db.FerramentasAcesso;
}

[ApiController]
[Route("api/[controller]")]
public class EtapasProcessosConfigController : BaseTenantCrudController<EtapaProcessoConfig>
{
    private readonly AppDbContext _db;
    public EtapasProcessosConfigController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<EtapaProcessoConfig> Set => _db.EtapasProcessosConfig;
}

[ApiController]
[Route("api/[controller]")]
public class VagasController : BaseTenantCrudController<Vaga>
{
    private readonly AppDbContext _db;
    public VagasController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<Vaga> Set => _db.Vagas;
}
