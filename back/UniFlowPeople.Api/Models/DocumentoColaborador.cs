using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class DocumentoColaborador
    {
        public int Id { get; set; }
        public int ColaboradorId { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NomeArquivo { get; set; } = string.Empty;
        public string UrlArquivo { get; set; } = string.Empty;
        public DateTime DataUpload { get; set; } = DateTime.UtcNow;
        public bool Obrigatorio { get; set; }
        public bool Validado { get; set; }
        public Colaborador Colaborador { get; set; } = null!;
    }
}