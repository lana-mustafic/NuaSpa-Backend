using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSistemskaNotifikacijaAndObavijest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Obavijesti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naslov = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tekst = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    SlikaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DatumObjave = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aktivna = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Obavijesti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SistemskaNotifikacije",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    Naslov = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tekst = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Procitana = table.Column<bool>(type: "bit", nullable: false),
                    RezervacijaId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemskaNotifikacije", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SistemskaNotifikacije_AspNetUsers_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SistemskaNotifikacije_Rezervacije_RezervacijaId",
                        column: x => x.RezervacijaId,
                        principalTable: "Rezervacije",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "94fb707f-2b00-43d7-b717-a51691b34ae9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "dd310383-8f30-4ace-90dd-d42a7dec20e2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "c849a8cb-547a-4d59-956c-fb665e4aecd5");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "b95260b4-464f-4e4f-b532-1e88b6e11a1a", "29b4e549-acac-4768-9a30-ac3fb046a8d7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ecc09def-88ca-4c04-b022-0c980cf494e5", "85a9540e-f36f-41a7-9740-4762e522ff70" });

            migrationBuilder.CreateIndex(
                name: "IX_Obavijesti_Aktivna_DatumObjave",
                table: "Obavijesti",
                columns: new[] { "Aktivna", "DatumObjave" });

            migrationBuilder.CreateIndex(
                name: "IX_SistemskaNotifikacije_KorisnikId_CreatedAt",
                table: "SistemskaNotifikacije",
                columns: new[] { "KorisnikId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SistemskaNotifikacije_KorisnikId_Procitana_CreatedAt",
                table: "SistemskaNotifikacije",
                columns: new[] { "KorisnikId", "Procitana", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SistemskaNotifikacije_RezervacijaId",
                table: "SistemskaNotifikacije",
                column: "RezervacijaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Obavijesti");

            migrationBuilder.DropTable(
                name: "SistemskaNotifikacije");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "703a2de1-0231-4a2f-961c-f0f2e4be5025");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "f1c8a2df-9ac0-40d4-9c28-c66f39d3f57e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "09ee6807-b121-4a8a-82b6-6065a86de6ec");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a1fb69f7-dc1b-4d13-bcf1-8f0ea9d5b85d", "502d123f-c708-4f4d-a6a0-ccb74e5b58ce" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "69ff5cc0-f86f-491b-8db1-31b7aaf208bb", "4df108b2-8873-4d81-882d-7c852726c2b7" });
        }
    }
}
