namespace UniFlowPeople.Api.Models
{
    public class AdmissaoDocumento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int AdmissaoProcessoId { get; set; }
        public string NomeArquivo { get; set; } = string.Empty;
        public string UrlArquivo { get; set; } = string.Empty;
        public string TipoArquivo { get; set; } = string.Empty;
        public DateTime DataUpload { get; set; } = DateTime.UtcNow;

        public Empresa Empresa { get; set; } = null!;
        public AdmissaoProcesso AdmissaoProcesso { get; set; } = null!;
    }
}
