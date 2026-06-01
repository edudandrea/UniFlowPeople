namespace UniFlowPeople.Api.Models
{
    public class Curriculo
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? NomeArquivo { get; set; }
        public string? CurriculoUrl { get; set; }
        public string Status { get; set; } = "Disponivel";
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public Empresa Empresa { get; set; } = null!;
    }
}
