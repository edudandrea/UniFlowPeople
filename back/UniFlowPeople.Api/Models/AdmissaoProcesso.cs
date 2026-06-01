namespace UniFlowPeople.Api.Models
{
    public class AdmissaoProcesso
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int? ColaboradorId { get; set; }
        public string NomeCandidato { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string CargoPretendido { get; set; } = string.Empty;
        public DateTime DataPrevistaAdmissao { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Em andamento";
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public Empresa Empresa { get; set; } = null!;
        public Colaborador? Colaborador { get; set; }
        public ICollection<AdmissaoEtapa> Etapas { get; set; } = new List<AdmissaoEtapa>();
        public ICollection<DocumentoInstitucional> DocumentosInstitucionais { get; set; } = new List<DocumentoInstitucional>();
    }
}
