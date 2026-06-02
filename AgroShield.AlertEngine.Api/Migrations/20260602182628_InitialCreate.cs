using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroShield.AlertEngine.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "terrenos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", nullable: false),
                    AreaTotalHectares = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AreaReservaHectares = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    AreaCultivoHectares = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    EmCultivo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CulturaAtual = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoSolo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IrrigacaoAtiva = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataReferencia = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Observacoes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terrenos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "historico_alertas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TerrenoId = table.Column<long>(type: "bigint", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Severidade = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MensagemParaFala = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MensagemTecnica = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AcaoRecomendada = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NdviMedio = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NdviZonaNorte = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    NdviZonaSul = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    UmidadeRelativa = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiasSemImagemSatelite = table.Column<int>(type: "int", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historico_alertas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_historico_alertas_terrenos_TerrenoId",
                        column: x => x.TerrenoId,
                        principalTable: "terrenos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_historico_alertas_Codigo",
                table: "historico_alertas",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_historico_alertas_CriadoEm",
                table: "historico_alertas",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_historico_alertas_TerrenoId",
                table: "historico_alertas",
                column: "TerrenoId");

            migrationBuilder.CreateIndex(
                name: "IX_terrenos_Nome",
                table: "terrenos",
                column: "Nome");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historico_alertas");

            migrationBuilder.DropTable(
                name: "terrenos");
        }
    }
}
