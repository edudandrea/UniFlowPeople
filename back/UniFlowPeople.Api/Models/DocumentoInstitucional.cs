namespace UniFlowPeople.Api.Models
{
    public class DocumentoInstitucional
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int? ColaboradorId { get; set; }
        public int? AdmissaoProcessoId { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataGeracao { get; set; } = DateTime.UtcNow;

        public Empresa Empresa { get; set; } = null!;
        public Colaborador? Colaborador { get; set; }
        public AdmissaoProcesso? AdmissaoProcesso { get; set; }
    }
}
