using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Beneficio
    {
        public int Id { get; set; }
    public int EmpresaId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal ValorPadrao { get; set; }
    public bool Ativo { get; set; } = true;

    public Empresa Empresa { get; set; } = null!;
    public ICollection<BeneficioColaborador> Colaboradores { get; set; } = new List<BeneficioColaborador>();
    }
}