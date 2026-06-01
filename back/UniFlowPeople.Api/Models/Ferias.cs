using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Ferias
    {
        public int Id { get; set; }
        public int ColaboradorId { get; set; }

        public DateTime PeriodoAquisitivoInicio { get; set; }
        public DateTime PeriodoAquisitivoFim { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public int Dias { get; set; }
        public bool Abono { get; set; }
        public string Status { get; set; } = "Pendente";

        public Colaborador Colaborador { get; set; } = null!;
    }
}