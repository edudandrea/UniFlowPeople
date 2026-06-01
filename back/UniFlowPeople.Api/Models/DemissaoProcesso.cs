namespace UniFlowPeople.Api.Models
{
    public class DemissaoProcesso
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int ColaboradorId { get; set; }
        public string TipoDemissao { get; set; } = "Sem justa causa";
        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataDesligamento { get; set; }
        public string Status { get; set; } = "Em andamento";
        public string? Motivo { get; set; }
        public string? Observacoes { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Colaborador Colaborador { get; set; } = null!;
        public ICollection<DemissaoEtapa> Etapas { get; set; } = new List<DemissaoEtapa>();
    }
}
