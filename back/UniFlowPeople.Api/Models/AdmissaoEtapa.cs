namespace UniFlowPeople.Api.Models
{
    public class AdmissaoEtapa
    {
        public int Id { get; set; }
        public int AdmissaoProcessoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Status { get; set; } = "Pendente";
        public DateTime? DataConclusao { get; set; }
        public string? Responsavel { get; set; }
        public string? Observacoes { get; set; }

        public AdmissaoProcesso AdmissaoProcesso { get; set; } = null!;
    }
}
