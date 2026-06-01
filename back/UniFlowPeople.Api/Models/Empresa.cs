using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? Endereco { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Cep { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public ICollection<Filial> Filiais { get; set; } = new List<Filial>();
        public ICollection<Departamento> Departamentos { get; set; } = new List<Departamento>();
        public ICollection<Cargo> Cargos { get; set; } = new List<Cargo>();
        public ICollection<Colaborador> Colaboradores { get; set; } = new List<Colaborador>();
        public ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();
    }
}
