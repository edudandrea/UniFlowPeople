namespace UniFlowPeople.Api.Models
{
    public class CargoEpi
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int CargoId { get; set; }
        public int EpiId { get; set; }
        public int QuantidadePadrao { get; set; } = 1;
        public bool Obrigatorio { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;
        public Cargo Cargo { get; set; } = null!;
        public Epi Epi { get; set; } = null!;
    }
}
