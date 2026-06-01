using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Filial
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string? Cnpj { get; set; }
        public string? Endereco { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Telefone { get; set; }
        public bool Ativa { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;
        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
    }
}