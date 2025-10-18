using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace athenasarchive.Migrations
{
    /// <inheritdoc />
    public partial class TornaSenhaNulaParaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notificacoes_Usuarios_UsuarioId1",
                table: "Notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_Notificacoes_UsuarioId1",
                table: "Notificacoes");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Notificacoes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "Notificacoes",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_UsuarioId1",
                table: "Notificacoes",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Notificacoes_Usuarios_UsuarioId1",
                table: "Notificacoes",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
