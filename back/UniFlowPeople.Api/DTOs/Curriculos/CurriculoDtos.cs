namespace UniFlowPeople.Api.DTOs.Curriculos;

public sealed class CurriculoUploadRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Status { get; set; }
    public IFormFile? Arquivo { get; set; }
}
