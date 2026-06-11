using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniFlowPeople.Api.Migrations
{
    [Migration("20260611120000_DocumentosInstitucionaisModelos")]
    public partial class DocumentosInstitucionaisModelos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DocumentoAdmissao",
                table: "DocumentosInstitucionais",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DocumentoDemissao",
                table: "DocumentosInstitucionais",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "DemissaoProcessoId",
                table: "DocumentosInstitucionais",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsModelo",
                table: "DocumentosInstitucionais",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModeloDocumentoId",
                table: "DocumentosInstitucionais",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeArquivoModelo",
                table: "DocumentosInstitucionais",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoArquivoModelo",
                table: "DocumentosInstitucionais",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrlArquivoModelo",
                table: "DocumentosInstitucionais",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosInstitucionais_DemissaoProcessoId",
                table: "DocumentosInstitucionais",
                column: "DemissaoProcessoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosInstitucionais_ModeloDocumentoId",
                table: "DocumentosInstitucionais",
                column: "ModeloDocumentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosInstitucionais_Demissoes_DemissaoProcessoId",
                table: "DocumentosInstitucionais",
                column: "DemissaoProcessoId",
                principalTable: "Demissoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosInstitucionais_DocumentosInstitucionais_ModeloDocumentoId",
                table: "DocumentosInstitucionais",
                column: "ModeloDocumentoId",
                principalTable: "DocumentosInstitucionais",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosInstitucionais_Demissoes_DemissaoProcessoId",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosInstitucionais_DocumentosInstitucionais_ModeloDocumentoId",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosInstitucionais_DemissaoProcessoId",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosInstitucionais_ModeloDocumentoId",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "DocumentoAdmissao",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "DocumentoDemissao",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "DemissaoProcessoId",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "IsModelo",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "ModeloDocumentoId",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "NomeArquivoModelo",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "TipoArquivoModelo",
                table: "DocumentosInstitucionais");

            migrationBuilder.DropColumn(
                name: "UrlArquivoModelo",
                table: "DocumentosInstitucionais");
        }
    }
}
