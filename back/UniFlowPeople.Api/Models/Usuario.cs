using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public int? EmpresaId { get; set; }
        public int? ColaboradorId { get; set; }

        public string Nome { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Colaborador";

        public bool Ativo { get; set; } = true;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public Empresa? Empresa { get; set; }
        public Colaborador? Colaborador { get; set; }
    }
}