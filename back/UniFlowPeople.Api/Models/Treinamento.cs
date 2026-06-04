namespace UniFlowPeople.Api.Models
{
    public class Treinamento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string Instrutor { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; } = DateTime.UtcNow;
        public DateTime? DataFim { get; set; }
        public int CargaHoraria { get; set; }
        public string? MetodoAvaliacaoEficacia { get; set; }
        public bool Eficaz { get; set; }
        public string Status { get; set; } = "Planejado";
        public bool Obrigatorio { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public ICollection<TreinamentoColaborador> Colaboradores { get; set; } = new List<TreinamentoColaborador>();
    }
}
