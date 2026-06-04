namespace UniFlowPeople.Api.Models
{
    public class Epi
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Ca { get; set; }
        public string? Descricao { get; set; }
        public int PeriodicidadeTrocaDias { get; set; } = 180;
        public bool Ativo { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;
        public ICollection<CargoEpi> Cargos { get; set; } = new List<CargoEpi>();
        public ICollection<ColaboradorEpi> Retiradas { get; set; } = new List<ColaboradorEpi>();
    }
}
