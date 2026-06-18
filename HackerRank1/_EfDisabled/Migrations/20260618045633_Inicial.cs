using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HackerRank1.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permiso",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permiso", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rol",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rol_permiso",
                columns: table => new
                {
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    permiso_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rol_permiso", x => new { x.rol_id, x.permiso_id });
                    table.ForeignKey(
                        name: "FK_rol_permiso_permiso_permiso_id",
                        column: x => x.permiso_id,
                        principalTable: "permiso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rol_permiso_rol_rol_id",
                        column: x => x.rol_id,
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    rol_id = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuario_rol_rol_id",
                        column: x => x.rol_id,
                        principalTable: "rol",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permiso",
                columns: new[] { "id", "clave", "descripcion" },
                values: new object[,]
                {
                    { 1, "donaciones.crear", "Crear donaciones" },
                    { 2, "donaciones.ver", "Ver donaciones" },
                    { 3, "donaciones.editar", "Editar donaciones" },
                    { 4, "donaciones.eliminar", "Eliminar donaciones" },
                    { 5, "beneficiarios.gestionar", "Gestionar beneficiarios" },
                    { 6, "inventario.gestionar", "Gestionar inventario" },
                    { 7, "gastos.gestionar", "Gestionar gastos" },
                    { 8, "usuarios.ver", "Ver usuarios" },
                    { 9, "usuarios.gestionar", "Gestionar usuarios" },
                    { 10, "usuarios.crear_admin", "Crear administradores" },
                    { 11, "roles.ver", "Ver roles" },
                    { 12, "roles.gestionar", "Gestionar roles" },
                    { 13, "roles.asignar", "Asignar roles a usuarios" }
                });

            migrationBuilder.InsertData(
                table: "rol",
                columns: new[] { "id", "creado_en", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rol básico de usuario", "usuario" },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rol de administrador operativo", "administrador" },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rol de superusuario con todos los permisos", "superusuario" }
                });

            migrationBuilder.InsertData(
                table: "rol_permiso",
                columns: new[] { "permiso_id", "rol_id" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 1, 2 },
                    { 2, 2 },
                    { 3, 2 },
                    { 4, 2 },
                    { 5, 2 },
                    { 6, 2 },
                    { 7, 2 },
                    { 8, 2 },
                    { 9, 2 },
                    { 11, 2 },
                    { 1, 3 },
                    { 2, 3 },
                    { 3, 3 },
                    { 4, 3 },
                    { 5, 3 },
                    { 6, 3 },
                    { 7, 3 },
                    { 8, 3 },
                    { 9, 3 },
                    { 10, 3 },
                    { 11, 3 },
                    { 12, 3 },
                    { 13, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_permiso_clave",
                table: "permiso",
                column: "clave",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rol_nombre",
                table: "rol",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rol_permiso_permiso_id",
                table: "rol_permiso",
                column: "permiso_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_rol_id",
                table: "usuario",
                column: "rol_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rol_permiso");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "permiso");

            migrationBuilder.DropTable(
                name: "rol");
        }
    }
}
