namespace UniFlowPeople.Api.DTOs.Usuarios;

public sealed class UsuarioCreateRequest
{
    public int? EmpresaId { get; set; }
    public int? ColaboradorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Role { get; set; } = "Colaborador";
    public bool Ativo { get; set; } = true;
}

public sealed class UsuarioUpdateRequest
{
    public int? ColaboradorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Senha { get; set; }
    public string Role { get; set; } = "Colaborador";
    public bool Ativo { get; set; } = true;
}
