namespace UniFlowPeople.Api.Models
{
    public class Cobranca
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int ContratoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataGeracao { get; set; } = DateTime.UtcNow;
        public DateTime DataVencimento { get; set; } = DateTime.UtcNow.AddDays(30);
        public string Status { get; set; } = "Pendente";

        public Empresa Empresa { get; set; } = null!;
        public Contrato Contrato { get; set; } = null!;
    }
}
