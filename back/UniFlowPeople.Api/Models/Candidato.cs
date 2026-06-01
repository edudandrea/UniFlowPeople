using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Candidato
    {
        public int Id { get; set; }
        public int VagaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Cpf { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? CurriculoUrl { get; set; }
        public string? Linkedin { get; set; }
        public string Status { get; set; } = "Recebido";
        public string? Observacoes { get; set; }
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public Vaga Vaga { get; set; } = null!;
    }
}