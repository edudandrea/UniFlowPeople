namespace UniFlowPeople.Api.Models
{
    public class Contrato
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int? PlanoId { get; set; }
        public string Plano { get; set; } = "Starter";
        public string Status { get; set; } = "Ativo";
        public DateTime DataInicio { get; set; } = DateTime.UtcNow;
        public DateTime? DataFim { get; set; }
        public int LimiteColaboradores { get; set; } = 50;
        public decimal ValorMensal { get; set; }
        public decimal ValorImplementacao { get; set; }
        public decimal MultaQuebraContrato { get; set; }
        public string? Observacoes { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Plano? PlanoCadastro { get; set; }
        public ICollection<Cobranca> Cobrancas { get; set; } = new List<Cobranca>();
    }
}
