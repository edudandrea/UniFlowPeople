using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class BeneficioColaborador
    {
        public int Id { get; set; }
        public int ColaboradorId { get; set; }
        public int BeneficioId { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Ativo { get; set; } = true;
        public Colaborador Colaborador { get; set; } = null!;
        public Beneficio Beneficio { get; set; } = null!;
    }
}