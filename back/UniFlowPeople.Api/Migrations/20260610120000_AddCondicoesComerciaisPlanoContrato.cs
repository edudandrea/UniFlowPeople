using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniFlowPeople.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCondicoesComerciaisPlanoContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MultaQuebraContrato",
                table: "Planos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorImplementacao",
                table: "Planos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MultaQuebraContrato",
                table: "Contratos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorImplementacao",
                table: "Contratos",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MultaQuebraContrato",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "ValorImplementacao",
                table: "Planos");

            migrationBuilder.DropColumn(
                name: "MultaQuebraContrato",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "ValorImplementacao",
                table: "Contratos");
        }
    }
}
