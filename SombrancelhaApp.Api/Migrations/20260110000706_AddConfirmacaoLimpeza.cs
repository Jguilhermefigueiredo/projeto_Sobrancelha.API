using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SombrancelhaApp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddConfirmacaoLimpeza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AtendimentoSimulacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClienteId = table.Column<string>(type: "TEXT", nullable: false),
                    NomeMolde = table.Column<string>(type: "TEXT", nullable: false),
                    CorHex = table.Column<string>(type: "TEXT", nullable: false),
                    CaminhoImagemFinal = table.Column<string>(type: "TEXT", nullable: false),
                    UrlImagemFinal = table.Column<string>(type: "TEXT", nullable: false),
                    ConfirmadoParaDeletar = table.Column<bool>(type: "INTEGER", nullable: false),
                    AvisadoSobreExpiracao = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtendimentoSimulacoes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtendimentoSimulacoes");
        }
    }
}
