using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace athenasarchive.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaModeracaoEDenuncias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Denuncias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Motivo = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AutorId = table.Column<int>(type: "INTEGER", nullable: false),
                    TopicoId = table.Column<int>(type: "INTEGER", nullable: true),
                    RespostaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Denuncias_Respostas_RespostaId",
                        column: x => x.RespostaId,
                        principalTable: "Respostas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Denuncias_Topicos_TopicoId",
                        column: x => x.TopicoId,
                        principalTable: "Topicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Denuncias_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LogsModeracao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Acao = table.Column<int>(type: "INTEGER", nullable: false),
                    Justificativa = table.Column<string>(type: "TEXT", nullable: false),
                    DataAcao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModeradorId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioAlvoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DenunciaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsModeracao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsModeracao_Denuncias_DenunciaId",
                        column: x => x.DenunciaId,
                        principalTable: "Denuncias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LogsModeracao_Usuarios_ModeradorId",
                        column: x => x.ModeradorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LogsModeracao_Usuarios_UsuarioAlvoId",
                        column: x => x.UsuarioAlvoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_AutorId",
                table: "Denuncias",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_RespostaId",
                table: "Denuncias",
                column: "RespostaId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_TopicoId",
                table: "Denuncias",
                column: "TopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsModeracao_DenunciaId",
                table: "LogsModeracao",
                column: "DenunciaId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsModeracao_ModeradorId",
                table: "LogsModeracao",
                column: "ModeradorId");

            migrationBuilder.CreateIndex(
                name: "IX_LogsModeracao_UsuarioAlvoId",
                table: "LogsModeracao",
                column: "UsuarioAlvoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogsModeracao");

            migrationBuilder.DropTable(
                name: "Denuncias");
        }
    }
}
