using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRezervacijaStateMachine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rezervacije_KorisnikId",
                table: "Rezervacije");

            migrationBuilder.AddColumn<int>(
                name: "OtkazaoUserId",
                table: "Rezervacije",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PotvrdioUserId",
                table: "Rezervacije",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PotvrdjenaAt",
                table: "Rezervacije",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SnimakCijena",
                table: "Rezervacije",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "SnimakTrajanjeMinuta",
                table: "Rezervacije",
                type: "int",
                nullable: false,
                defaultValue: 60);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Rezervacije",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ZavrsenaAt",
                table: "Rezervacije",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ZavrsioUserId",
                table: "Rezervacije",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RezervacijaStatusPromjene",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RezervacijaId = table.Column<int>(type: "int", nullable: false),
                    FromStatus = table.Column<int>(type: "int", nullable: false),
                    ToStatus = table.Column<int>(type: "int", nullable: false),
                    ActorUserId = table.Column<int>(type: "int", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RezervacijaStatusPromjene", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RezervacijaStatusPromjene_Rezervacije_RezervacijaId",
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
                value: "0964540f-3f7f-4787-a343-b8f3c55187cc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "8822c881-ab03-4f04-b0c0-9f3149ad9b8a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "79bae97a-207b-4e8e-a767-779b2f42ecc8");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f40fcbab-af3d-44d9-9531-8c0deb176ba0", "0689e144-6636-4436-8b06-57b8efb73820" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "d53171c5-4ba2-4d72-bac3-219836142f7e", "f1eb1c70-3b47-4a93-85b8-aa3f1c123039" });

            migrationBuilder.Sql("""
                UPDATE r
                SET
                    SnimakCijena = u.Cijena,
                    SnimakTrajanjeMinuta = CASE WHEN u.TrajanjeMinuta > 0 THEN u.TrajanjeMinuta ELSE 60 END,
                    Status = CASE
                        WHEN r.IsOtkazana = 1 THEN 2
                        WHEN r.IsPotvrdjena = 1 THEN 1
                        ELSE 0
                    END
                FROM Rezervacije r
                INNER JOIN Usluge u ON u.Id = r.UslugaId;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_KorisnikId_ZaposlenikId_DatumRezervacije",
                table: "Rezervacije",
                columns: new[] { "KorisnikId", "ZaposlenikId", "DatumRezervacije" },
                unique: true,
                filter: "[Status] <> 2");

            migrationBuilder.CreateIndex(
                name: "IX_RezervacijaStatusPromjene_CreatedAt",
                table: "RezervacijaStatusPromjene",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RezervacijaStatusPromjene_RezervacijaId",
                table: "RezervacijaStatusPromjene",
                column: "RezervacijaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RezervacijaStatusPromjene");

            migrationBuilder.DropIndex(
                name: "IX_Rezervacije_KorisnikId_ZaposlenikId_DatumRezervacije",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "OtkazaoUserId",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "PotvrdioUserId",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "PotvrdjenaAt",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "SnimakCijena",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "SnimakTrajanjeMinuta",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "ZavrsenaAt",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "ZavrsioUserId",
                table: "Rezervacije");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "1beb32a9-e4e3-4d05-80b4-0d651fcbea98");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "017b1c6d-fbae-4f4f-b1ca-ec829b28e63a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "789ae94f-669d-4b2a-92af-c9e98ad45b1c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "66c1a2f6-2f34-40f5-9881-066d5d069a09", "47abb512-5f38-45b9-a3e2-a96742d2a774" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7c074346-0812-414c-b3c9-4f638676abad", "d10ddd65-341e-448b-9b95-18b545c423b1" });

            migrationBuilder.CreateIndex(
                name: "IX_Rezervacije_KorisnikId",
                table: "Rezervacije",
                column: "KorisnikId");
        }
    }
}
