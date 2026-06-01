using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Departamento
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int? GestorId { get; set; }
        public bool Ativo { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;
        public Colaborador? Gestor { get; set; }
        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
    }
}