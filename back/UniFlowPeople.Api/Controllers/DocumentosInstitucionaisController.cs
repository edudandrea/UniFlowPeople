using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniFlowPeople.Api.Data;
using UniFlowPeople.Api.Models;
using UniFlowPeople.Api.Services.Tenancy;

namespace UniFlowPeople.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentosInstitucionaisController : BaseTenantCrudController<DocumentoInstitucional>
{
    private readonly AppDbContext _db;
    public DocumentosInstitucionaisController(AppDbContext db, ITenantContext tenant) : base(db, tenant) => _db = db;
    protected override DbSet<DocumentoInstitucional> Set => _db.DocumentosInstitucionais;
}
