namespace UniFlowPeople.Api.Models
{
    public class Plano
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int PrazoDias { get; set; } = 30;
        public int LimiteColaboradores { get; set; } = 50;
        public decimal ValorCobranca { get; set; }
        public decimal ValorImplementacao { get; set; }
        public decimal MultaQuebraContrato { get; set; }
        public string Status { get; set; } = "Ativo";
        public string? Observacoes { get; set; }

        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}
