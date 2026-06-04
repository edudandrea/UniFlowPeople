namespace UniFlowPeople.Api.Models
{
    public class EtapaProcessoConfig
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string TipoProcesso { get; set; } = "Admissao";
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int Ordem { get; set; } = 1;
        public bool PrimeiraEtapaConcluida { get; set; }
        public bool Ativa { get; set; } = true;

        public Empresa Empresa { get; set; } = null!;
    }
}
