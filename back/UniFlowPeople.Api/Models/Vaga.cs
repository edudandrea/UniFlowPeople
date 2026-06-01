using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Vaga
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int? DepartamentoId { get; set; }
        public int? CargoId { get; set; }

        public string Titulo { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int Quantidade { get; set; }
        public decimal? Salario { get; set; }
        public string Status { get; set; } = "Aberta";

        public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
        public DateTime? DataEncerramento { get; set; }

        public Empresa Empresa { get; set; } = null!;
        public Departamento? Departamento { get; set; }
        public Cargo? Cargo { get; set; }

        public ICollection<Candidato> Candidatos { get; set; } = new List<Candidato>();
    }
}