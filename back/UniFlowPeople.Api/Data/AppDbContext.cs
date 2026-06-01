using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniFlowPeople.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace UniFlowPeople.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Filial> Filiais { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }
        public DbSet<Cargo> Cargos { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<RegistroPonto> RegistrosPonto { get; set; }
        public DbSet<Ferias> Ferias { get; set; }
        public DbSet<Beneficio> Beneficios { get; set; }
        public DbSet<BeneficioColaborador> BeneficiosColaboradores { get; set; }
        public DbSet<DocumentoColaborador> DocumentosColaboradores { get; set; }
        public DbSet<Vaga> Vagas { get; set; }
        public DbSet<Candidato> Candidatos { get; set; }
        public DbSet<Contrato> Contratos { get; set; }
        public DbSet<Curriculo> Curriculos { get; set; }
        public DbSet<Treinamento> Treinamentos { get; set; }
        public DbSet<TreinamentoColaborador> TreinamentosColaboradores { get; set; }
        public DbSet<AdmissaoProcesso> Admissoes { get; set; }
        public DbSet<AdmissaoEtapa> AdmissaoEtapas { get; set; }
        public DbSet<DocumentoInstitucional> DocumentosInstitucionais { get; set; }
        public DbSet<DemissaoProcesso> Demissoes { get; set; }
        public DbSet<DemissaoEtapa> DemissaoEtapas { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizarDatasUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            NormalizarDatasUtc();
            return base.SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigurarDatasUtc(modelBuilder);

            modelBuilder.Entity<Empresa>()
                .HasIndex(x => x.Cnpj)
                .IsUnique();

            modelBuilder.Entity<Colaborador>()
                .HasIndex(x => new { x.EmpresaId, x.Cpf })
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(x => x.Login)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Departamento>()
                .HasOne(x => x.Gestor)
                .WithMany()
                .HasForeignKey(x => x.GestorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Colaborador>()
                .Property(x => x.Salario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Cargo>()
                .Property(x => x.SalarioBase)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Beneficio>()
                .Property(x => x.ValorPadrao)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BeneficioColaborador>()
                .Property(x => x.Valor)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Vaga>()
                .Property(x => x.Salario)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Contrato>()
                .Property(x => x.ValorMensal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Curriculo>()
                .HasIndex(x => new { x.EmpresaId, x.Email });

            modelBuilder.Entity<TreinamentoColaborador>()
                .HasIndex(x => new { x.TreinamentoId, x.ColaboradorId })
                .IsUnique();

            modelBuilder.Entity<TreinamentoColaborador>()
                .HasOne(x => x.Treinamento)
                .WithMany(x => x.Colaboradores)
                .HasForeignKey(x => x.TreinamentoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TreinamentoColaborador>()
                .HasOne(x => x.Colaborador)
                .WithMany()
                .HasForeignKey(x => x.ColaboradorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AdmissaoProcesso>()
                .HasOne(x => x.Colaborador)
                .WithMany()
                .HasForeignKey(x => x.ColaboradorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AdmissaoEtapa>()
                .HasOne(x => x.AdmissaoProcesso)
                .WithMany(x => x.Etapas)
                .HasForeignKey(x => x.AdmissaoProcessoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentoInstitucional>()
                .HasOne(x => x.AdmissaoProcesso)
                .WithMany(x => x.DocumentosInstitucionais)
                .HasForeignKey(x => x.AdmissaoProcessoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DemissaoProcesso>()
                .HasOne(x => x.Colaborador)
                .WithMany()
                .HasForeignKey(x => x.ColaboradorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DemissaoEtapa>()
                .HasOne(x => x.DemissaoProcesso)
                .WithMany(x => x.Etapas)
                .HasForeignKey(x => x.DemissaoProcessoId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void NormalizarDatasUtc()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;

                foreach (var property in entry.Properties)
                {
                    if (property.CurrentValue is DateTime date)
                    {
                        property.CurrentValue = ToUtc(date);
                    }
                }
            }
        }

        private static DateTime ToUtc(DateTime date)
        {
            if (date.Kind == DateTimeKind.Utc) return date;
            if (date.Kind == DateTimeKind.Local) return date.ToUniversalTime();
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        private static DateTime FromDatabaseUtc(DateTime date) =>
            DateTime.SpecifyKind(date, DateTimeKind.Utc);

        private static void ConfigurarDatasUtc(ModelBuilder modelBuilder)
        {
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                value => ToUtc(value),
                value => FromDatabaseUtc(value));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                value => value.HasValue ? ToUtc(value.Value) : value,
                value => value.HasValue ? FromDatabaseUtc(value.Value) : value);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                    }
                }
            }
        }
    }
}
