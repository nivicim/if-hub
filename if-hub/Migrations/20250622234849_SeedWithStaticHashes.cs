using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace athenasarchive.Migrations
{
    /// <inheritdoc />
    public partial class SeedWithStaticHashes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "DataCriacao", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 22, 20, 30, 0, 0, DateTimeKind.Utc), "Discussões sobre linguagens, algoritmos e desenvolvimento de software.", "Programação" },
                    { 2, new DateTime(2025, 6, 22, 20, 30, 0, 0, DateTimeKind.Utc), "Tópicos sobre limites, derivadas, integrais e aplicações.", "Cálculo" },
                    { 3, new DateTime(2025, 6, 22, 20, 30, 0, 0, DateTimeKind.Utc), "Discussões sobre componentes de computador, montagem e manutenção.", "Hardware" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nome" },
                values: new object[,]
                {
                    { 1, "Aluno" },
                    { 2, "Moderador" },
                    { 3, "Admin" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Ativo", "DataCriacao", "Email", "Nome", "RoleId", "SenhaHash" },
                values: new object[,]
                {
                    { 1, true, new DateTime(2025, 6, 22, 20, 30, 0, 0, DateTimeKind.Utc), "admin@email.com", "Admin User", 3, "AQAAAAIAAYagAAAAEPhH/65zZtC4N9YtCqY9iE2n5xZ3zZ+yZ+e8w3c9X6b8V5n7f7j3X6n8Y7d9V6c3" },
                    { 2, true, new DateTime(2025, 6, 22, 20, 30, 0, 0, DateTimeKind.Utc), "aluno@email.com", "Aluno Teste", 1, "AQAAAAIAAYagAAAAEJ5y/8f6zZtC4N9YtCqY9iE2n5xZ3zZ+yZ+e8w3c9X6b8V5n7f7j3X6n8Y7d9V6c4" }
                });

            migrationBuilder.InsertData(
                table: "Topicos",
                columns: new[] { "Id", "CategoriaId", "Conteudo", "DataCriacao", "EditadoEm", "Titulo", "UsuarioId" },
                values: new object[] { 1, 1, "Olá pessoal, estou com dificuldade para entender como funcionam os ponteiros para ponteiros em C++. Alguém poderia me dar uma luz?", new DateTime(2025, 6, 22, 20, 30, 0, 0, DateTimeKind.Utc), null, "Dúvida sobre ponteiros em C++", 2 });

            migrationBuilder.InsertData(
                table: "Respostas",
                columns: new[] { "Id", "Conteudo", "DataCriacao", "EditadoEm", "RespostaPaiId", "TopicoId", "UsuarioId" },
                values: new object[] { 1, "Claro! Um ponteiro para ponteiro armazena o endereço de memória de outro ponteiro. Pense nele como um 'índice' para seus 'índices' de memória.", new DateTime(2025, 6, 22, 20, 30, 0, 0, DateTimeKind.Utc), null, null, 1, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Respostas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Topicos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categorias",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
