namespace UniFlowPeople.Api.Models
{
    public class DocumentoInstitucional
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int? ColaboradorId { get; set; }
        public int? AdmissaoProcessoId { get; set; }
        public int? DemissaoProcessoId { get; set; }
        public int? ModeloDocumentoId { get; set; }
        public bool IsModelo { get; set; }
        public bool DocumentoAdmissao { get; set; }
        public bool DocumentoDemissao { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
        public string? NomeArquivoModelo { get; set; }
        public string? TipoArquivoModelo { get; set; }
        public string? UrlArquivoModelo { get; set; }
        public DateTime DataGeracao { get; set; } = DateTime.UtcNow;

        public Empresa Empresa { get; set; } = null!;
        public Colaborador? Colaborador { get; set; }
        public AdmissaoProcesso? AdmissaoProcesso { get; set; }
        public DemissaoProcesso? DemissaoProcesso { get; set; }
        public DocumentoInstitucional? ModeloDocumento { get; set; }
    }
}
