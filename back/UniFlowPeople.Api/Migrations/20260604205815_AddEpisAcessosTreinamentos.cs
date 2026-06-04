using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UniFlowPeople.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisAcessosTreinamentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Eficaz",
                table: "Treinamentos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetodoAvaliacaoEficacia",
                table: "Treinamentos",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Epis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Ca = table.Column<string>(type: "text", nullable: true),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    PeriodicidadeTrocaDias = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Epis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Epis_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FerramentasAcesso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Identificador = table.Column<string>(type: "text", nullable: true),
                    Descricao = table.Column<string>(type: "text", nullable: true),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FerramentasAcesso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FerramentasAcesso_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CargosEpis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    CargoId = table.Column<int>(type: "integer", nullable: false),
                    EpiId = table.Column<int>(type: "integer", nullable: false),
                    QuantidadePadrao = table.Column<int>(type: "integer", nullable: false),
                    Obrigatorio = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargosEpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CargosEpis_Cargos_CargoId",
                        column: x => x.CargoId,
                        principalTable: "Cargos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CargosEpis_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CargosEpis_Epis_EpiId",
                        column: x => x.EpiId,
                        principalTable: "Epis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ColaboradoresEpis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ColaboradorId = table.Column<int>(type: "integer", nullable: false),
                    EpiId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    DataRetirada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPrevistaTroca = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataDevolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColaboradoresEpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColaboradoresEpis_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColaboradoresEpis_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColaboradoresEpis_Epis_EpiId",
                        column: x => x.EpiId,
                        principalTable: "Epis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ColaboradoresFerramentasAcesso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EmpresaId = table.Column<int>(type: "integer", nullable: false),
                    ColaboradorId = table.Column<int>(type: "integer", nullable: false),
                    FerramentaAcessoId = table.Column<int>(type: "integer", nullable: false),
                    DataEntrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataDevolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColaboradoresFerramentasAcesso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ColaboradoresFerramentasAcesso_Colaboradores_ColaboradorId",
                        column: x => x.ColaboradorId,
                        principalTable: "Colaboradores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColaboradoresFerramentasAcesso_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ColaboradoresFerramentasAcesso_FerramentasAcesso_Ferramenta~",
                        column: x => x.FerramentaAcessoId,
                        principalTable: "FerramentasAcesso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargosEpis_CargoId",
                table: "CargosEpis",
                column: "CargoId");

            migrationBuilder.CreateIndex(
                name: "IX_CargosEpis_EmpresaId_CargoId_EpiId",
                table: "CargosEpis",
                columns: new[] { "EmpresaId", "CargoId", "EpiId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargosEpis_EpiId",
                table: "CargosEpis",
                column: "EpiId");

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradoresEpis_ColaboradorId",
                table: "ColaboradoresEpis",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradoresEpis_EmpresaId",
                table: "ColaboradoresEpis",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradoresEpis_EpiId",
                table: "ColaboradoresEpis",
                column: "EpiId");

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradoresFerramentasAcesso_ColaboradorId",
                table: "ColaboradoresFerramentasAcesso",
                column: "ColaboradorId");

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradoresFerramentasAcesso_EmpresaId",
                table: "ColaboradoresFerramentasAcesso",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ColaboradoresFerramentasAcesso_FerramentaAcessoId",
                table: "ColaboradoresFerramentasAcesso",
                column: "FerramentaAcessoId");

            migrationBuilder.CreateIndex(
                name: "IX_Epis_EmpresaId",
                table: "Epis",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_FerramentasAcesso_EmpresaId",
                table: "FerramentasAcesso",
                column: "EmpresaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CargosEpis");

            migrationBuilder.DropTable(
                name: "ColaboradoresEpis");

            migrationBuilder.DropTable(
                name: "ColaboradoresFerramentasAcesso");

            migrationBuilder.DropTable(
                name: "Epis");

            migrationBuilder.DropTable(
                name: "FerramentasAcesso");

            migrationBuilder.DropColumn(
                name: "Eficaz",
                table: "Treinamentos");

            migrationBuilder.DropColumn(
                name: "MetodoAvaliacaoEficacia",
                table: "Treinamentos");
        }
    }
}
