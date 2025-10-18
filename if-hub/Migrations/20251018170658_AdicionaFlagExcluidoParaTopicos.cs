using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace athenasarchive.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaFlagExcluidoParaTopicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Excluido",
                table: "Topicos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Topicos",
                keyColumn: "Id",
                keyValue: 1,
                column: "Excluido",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Excluido",
                table: "Topicos");
        }
    }
}
