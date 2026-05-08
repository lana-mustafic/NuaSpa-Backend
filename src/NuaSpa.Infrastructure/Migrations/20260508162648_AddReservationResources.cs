using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProstorijaId",
                table: "Rezervacije",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RezervacijeOprema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RezervacijaId = table.Column<int>(type: "int", nullable: false),
                    OpremaId = table.Column<int>(type: "int", nullable: false),
                    Kolicina = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RezervacijeOprema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RezervacijeOprema_Oprema_OpremaId",
                        column: x => x.OpremaId,
                        principalTable: "Oprema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RezervacijeOprema_Rezervacije_RezervacijaId",
                        column: x => x.RezervacijaId,
                        principalTable: "Rezervacije",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "fa094f92-ebde-4bb2-8d98-d733d4ab4ee0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "c974e00a-d76b-4b45-a3ee-d7158dbe7de9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "912c0ff7-1e52-4bea-be2e-8dbfe1d3fe1f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6649fd8b-61c7-4f7a-b1dc-9112ddd609c9", "fdefcf09-c18c-4481-96a3-33ec97f7f919" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6a9bc4c9-fe43-4de5-85d4-56c3f4d7c89b", "61ffcf57-b607-4ec6-91e3-e1ca6f67215d" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_ProstorijaId",
                table: "Rezervacije",
                column: "ProstorijaId");

            migrationBuilder.CreateIndex(
                name: "IX_RezervacijeOprema_OpremaId",
                table: "RezervacijeOprema",
                column: "OpremaId");

            migrationBuilder.CreateIndex(
                name: "IX_RezervacijeOprema_RezervacijaId_OpremaId",
                table: "RezervacijeOprema",
                columns: new[] { "RezervacijaId", "OpremaId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Prostorije_ProstorijaId",
                table: "Rezervacije",
                column: "ProstorijaId",
                principalTable: "Prostorije",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Prostorije_ProstorijaId",
                table: "Rezervacije");

            migrationBuilder.DropTable(
                name: "RezervacijeOprema");

            migrationBuilder.DropIndex(
                name: "IX_Rezervacije_ProstorijaId",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "ProstorijaId",
                table: "Rezervacije");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "184662ac-951d-43cd-be69-12668f010dc4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "1fa44d69-9c6b-480b-9f0b-b1ac2f271718");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "0cd472dd-c767-4abf-92b2-64ec5bb76f3f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f55a5ef9-a544-437d-b23f-b9c298c37de4", "83c33ab9-433a-4111-94b6-3e1e4a122aa3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4c0acb28-2c9a-4e9a-9070-c892dce3c62c", "4d113796-0429-4db3-be99-a113f70ffc61" });
        }
    }
}
