namespace UniFlowPeople.Api.Models
{
    public class TreinamentoColaborador
    {
        public int Id { get; set; }
        public int TreinamentoId { get; set; }
        public int ColaboradorId { get; set; }
        public bool Presente { get; set; }
        public DateTime? DataPresenca { get; set; }
        public string Status { get; set; } = "Inscrito";
        public string? Observacoes { get; set; }

        public Treinamento Treinamento { get; set; } = null!;
        public Colaborador Colaborador { get; set; } = null!;
    }
}
