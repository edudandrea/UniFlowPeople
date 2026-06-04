namespace UniFlowPeople.Api.Models
{
    public class FerramentaAcesso
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Tag de acesso";
        public string? Identificador { get; set; }
        public string? Descricao { get; set; }
        public bool Ativa { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;
        public ICollection<ColaboradorFerramentaAcesso> Colaboradores { get; set; } = new List<ColaboradorFerramentaAcesso>();
    }
}
