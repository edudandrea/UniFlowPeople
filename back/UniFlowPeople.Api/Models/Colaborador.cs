using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniFlowPeople.Api.Models
{
    public class Colaborador
    {
        public int Id { get; set; }

    public int EmpresaId { get; set; }
    public int? FilialId { get; set; }
    public int? DepartamentoId { get; set; }
    public int? CargoId { get; set; }

    public string Nome { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string? Rg { get; set; }
    public string? Telefone { get; set; }
    public string? Email { get; set; }

    public string? Endereco { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public string? Cep { get; set; }

    public DateTime? DataNascimento { get; set; }
    public string? Sexo { get; set; }
    public string? EstadoCivil { get; set; }

    public DateTime DataAdmissao { get; set; }
    public DateTime? DataDemissao { get; set; }

    public string? TipoContrato { get; set; }
    public decimal Salario { get; set; }
    public int CargaHorariaSemanal { get; set; }

    public string? Matricula { get; set; }
    public string? Pis { get; set; }
    public string? Ctps { get; set; }

    public string Status { get; set; } = "Ativo";
    public string? FotoUrl { get; set; }
    public string? Observacoes { get; set; }

    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public Empresa Empresa { get; set; } = null!;
    public Filial? Filial { get; set; }
    public Departamento? Departamento { get; set; }
    public Cargo? Cargo { get; set; }

    public ICollection<DocumentoColaborador> Documentos { get; set; } = new List<DocumentoColaborador>();
    public ICollection<RegistroPonto> RegistrosPonto { get; set; } = new List<RegistroPonto>();
    public ICollection<Ferias> Ferias { get; set; } = new List<Ferias>();
    public ICollection<BeneficioColaborador> Beneficios { get; set; } = new List<BeneficioColaborador>();
    public ICollection<ColaboradorEpi> Epis { get; set; } = new List<ColaboradorEpi>();
    public ICollection<ColaboradorFerramentaAcesso> FerramentasAcesso { get; set; } = new List<ColaboradorFerramentaAcesso>();

    }
}
