using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpaResources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpaCentri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Telefon = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Opis = table.Column<string>(type: "nvarchar(1200)", maxLength: 1200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpaCentri", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oprema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpaCentarId = table.Column<int>(type: "int", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Napomena = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Kolicina = table.Column<int>(type: "int", nullable: false),
                    IsIspravna = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oprema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Oprema_SpaCentri_SpaCentarId",
                        column: x => x.SpaCentarId,
                        principalTable: "SpaCentri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prostorije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpaCentarId = table.Column<int>(type: "int", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Kapacitet = table.Column<int>(type: "int", nullable: false),
                    IsAktivna = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prostorije", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prostorije_SpaCentri_SpaCentarId",
                        column: x => x.SpaCentarId,
                        principalTable: "SpaCentri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RadnaVremena",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpaCentarId = table.Column<int>(type: "int", nullable: false),
                    DanUSedmici = table.Column<int>(type: "int", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    OtvaraMin = table.Column<int>(type: "int", nullable: true),
                    ZatvaraMin = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadnaVremena", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadnaVremena_SpaCentri_SpaCentarId",
                        column: x => x.SpaCentarId,
                        principalTable: "SpaCentri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.InsertData(
                table: "SpaCentri",
                columns: new[] { "Id", "Adresa", "CreatedAt", "Email", "IsDeleted", "Naziv", "Opis", "Telefon" },
                values: new object[] { 1, "Sarajevo", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "info@nuaspa.ba", false, "NuaSpa", "Luksuzni spa centar. Relax. Renew. Rejuvenate.", "033 000 000" });

            migrationBuilder.InsertData(
                table: "RadnaVremena",
                columns: new[] { "Id", "CreatedAt", "DanUSedmici", "IsClosed", "IsDeleted", "OtvaraMin", "SpaCentarId", "ZatvaraMin" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, false, 540, 1, 1020 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, false, false, 540, 1, 1020 },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, false, false, 540, 1, 1020 },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, false, false, 540, 1, 1020 },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, false, false, 540, 1, 1020 },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 6, true, false, null, 1, null },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 7, true, false, null, 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Oprema_SpaCentarId",
                table: "Oprema",
                column: "SpaCentarId");

            migrationBuilder.CreateIndex(
                name: "IX_Prostorije_SpaCentarId",
                table: "Prostorije",
                column: "SpaCentarId");

            migrationBuilder.CreateIndex(
                name: "IX_RadnaVremena_SpaCentarId",
                table: "RadnaVremena",
                column: "SpaCentarId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Oprema");

            migrationBuilder.DropTable(
                name: "Prostorije");

            migrationBuilder.DropTable(
                name: "RadnaVremena");

            migrationBuilder.DropTable(
                name: "SpaCentri");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "8a375e95-21da-410a-b55d-34129d9684c2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "bfd2a069-0236-4c14-aca7-0ce04c12ad97");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "8c343046-d84b-4b88-a3fb-fac67323e843");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a730d1d2-d5cd-49ea-9675-96eddc3b40ee", "2daa28c1-e051-4456-8f35-55170d7dad22" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "fca6684d-3858-481a-bfe3-f2677291c586", "09efd599-0b2c-4718-b8cc-a914327440fa" });
        }
    }
}
