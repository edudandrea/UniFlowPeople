namespace UniFlowPeople.Api.Models
{
    public class ColaboradorFerramentaAcesso
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int ColaboradorId { get; set; }
        public int FerramentaAcessoId { get; set; }
        public DateTime DataEntrega { get; set; } = DateTime.UtcNow;
        public DateTime? DataDevolucao { get; set; }
        public string Status { get; set; } = "Entregue";
        public string? Observacoes { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Colaborador Colaborador { get; set; } = null!;
        public FerramentaAcesso FerramentaAcesso { get; set; } = null!;
    }
}
