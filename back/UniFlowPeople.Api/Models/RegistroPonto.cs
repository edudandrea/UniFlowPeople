using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class RegistroPonto
    {
        public int Id { get; set; }
        public int ColaboradorId { get; set; }

        public DateTime DataHora { get; set; }
        public string Tipo { get; set; } = string.Empty;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Ip { get; set; }
        public string? Observacao { get; set; }

        public Colaborador Colaborador { get; set; } = null!;
    }
}