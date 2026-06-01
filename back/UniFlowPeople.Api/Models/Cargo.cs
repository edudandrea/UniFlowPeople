using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Cargo
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public string? Nivel { get; set; }
        public decimal SalarioBase { get; set; }
        public bool Ativo { get; set; } = true;
        public Empresa Empresa { get; set; } = null!;
        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
    }
}