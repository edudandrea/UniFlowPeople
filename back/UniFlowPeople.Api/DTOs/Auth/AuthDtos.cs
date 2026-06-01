namespace UniFlowPeople.Api.DTOs.Auth;

public sealed class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public sealed class BootstrapAdminRequest
{
    public string Nome { get; set; } = "Administrador do Sistema";
    public string Login { get; set; } = "admin";
    public string Email { get; set; } = "admin@uniflowpeople.com";
    public string Senha { get; set; } = "Admin@123";
}

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UsuarioInfo Usuario { get; set; } = new();
}

public sealed class UsuarioInfo
{
    public int Id { get; set; }
    public int? EmpresaId { get; set; }
    public int? ColaboradorId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public sealed class UpdateProfileRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class ChangePasswordRequest
{
    public string SenhaAtual { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
