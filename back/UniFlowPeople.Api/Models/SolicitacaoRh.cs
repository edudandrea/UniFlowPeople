namespace UniFlowPeople.Api.Models
{
    public class SolicitacaoRh
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int? ColaboradorId { get; set; }
        public int? SolicitanteUsuarioId { get; set; }

        public string Tipo { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Prioridade { get; set; } = "Normal";
        public string Status { get; set; } = "Enviada";
        public string? RespostaRh { get; set; }

        public DateTime DataSolicitacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataAtualizacao { get; set; }
        public DateTime? DataConclusao { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Colaborador? Colaborador { get; set; }
    }
}
