namespace UniFlowPeople.Api.Models
{
    public class ColaboradorEpi
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int ColaboradorId { get; set; }
        public int EpiId { get; set; }
        public int Quantidade { get; set; } = 1;
        public DateTime DataRetirada { get; set; } = DateTime.UtcNow;
        public DateTime? DataPrevistaTroca { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public string Status { get; set; } = "Retirado";
        public string? Observacoes { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Colaborador Colaborador { get; set; } = null!;
        public Epi Epi { get; set; } = null!;
    }
}
