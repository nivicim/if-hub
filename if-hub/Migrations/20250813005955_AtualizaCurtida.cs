using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace athenasarchive.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaCurtida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Curtidas_Respostas_RespostaId",
                table: "Curtidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Curtidas_Topicos_TopicoId",
                table: "Curtidas");

            migrationBuilder.AddForeignKey(
                name: "FK_Curtidas_Respostas_RespostaId",
                table: "Curtidas",
                column: "RespostaId",
                principalTable: "Respostas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Curtidas_Topicos_TopicoId",
                table: "Curtidas",
                column: "TopicoId",
                principalTable: "Topicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Curtidas_Respostas_RespostaId",
                table: "Curtidas");

            migrationBuilder.DropForeignKey(
                name: "FK_Curtidas_Topicos_TopicoId",
                table: "Curtidas");

            migrationBuilder.AddForeignKey(
                name: "FK_Curtidas_Respostas_RespostaId",
                table: "Curtidas",
                column: "RespostaId",
                principalTable: "Respostas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Curtidas_Topicos_TopicoId",
                table: "Curtidas",
                column: "TopicoId",
                principalTable: "Topicos",
                principalColumn: "Id");
        }
    }
}
