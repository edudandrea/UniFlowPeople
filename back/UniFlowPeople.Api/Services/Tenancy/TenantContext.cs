using System.Security.Claims;
using UniFlowPeople.Api.Services.Auth;

namespace UniFlowPeople.Api.Services.Tenancy;

public interface ITenantContext
{
    int? EmpresaId { get; }
    int? UsuarioId { get; }
    int? ColaboradorId { get; }
    string Role { get; }
    bool IsSistemaAdmin { get; }
}

public sealed class TenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public int? EmpresaId => int.TryParse(User?.FindFirstValue("empresaId"), out var id) ? id : null;
    public int? UsuarioId => int.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    public int? ColaboradorId => int.TryParse(User?.FindFirstValue("colaboradorId"), out var id) ? id : null;
    public string Role => User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    public bool IsSistemaAdmin => Role == Roles.SistemaAdmin;
}
