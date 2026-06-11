using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddZaposlenikBioAndSlikaUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "RezervacijeOprema",
                comment: "Medutabela: rezervacija ? oprema",
                oldComment: "Medutabela: rezervacija ? oprema");

            migrationBuilder.AlterTable(
                name: "Favoriti",
                comment: "Medutabela: korisnik ? omiljena usluga",
                oldComment: "Medutabela: korisnik ? omiljena usluga");

            migrationBuilder.AddColumn<string>(
                name: "SlikaUrl",
                table: "Usluge",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SlikaUrl",
                table: "Obavijesti",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "a2ea7651-c2af-4316-abdf-cdfd93dfbf6b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "7f4dc512-44e7-4f8c-863d-ea1a91f9812f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "2f5339ce-c617-4925-87ff-11466984a213");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "53c08bde-129e-4289-9e60-e90153177ea6", "e308bfca-f726-439d-9c2d-55b53ec4dbdf" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "194c0704-ae92-4928-9c44-8f88623fe260", "e547ee97-7110-435e-952b-21c7b17d4fa6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlikaUrl",
                table: "Usluge");

            migrationBuilder.DropColumn(
                name: "SlikaUrl",
                table: "Obavijesti");

            migrationBuilder.AlterTable(
                name: "RezervacijeOprema",
                comment: "Medutabela: rezervacija ? oprema",
                oldComment: "Medutabela: rezervacija ? oprema");

            migrationBuilder.AlterTable(
                name: "Favoriti",
                comment: "Medutabela: korisnik ? omiljena usluga",
                oldComment: "Medutabela: korisnik ? omiljena usluga");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "1daf736d-f71e-4169-9a21-fa1bad3dc0ae");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "6434360f-b04f-40f7-a2f3-cc68ffeda5aa");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "41190a94-84e6-4c85-b2b7-11f7255114f1");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "18fd0fd2-4379-4948-a904-47fac4f554ff", "7930b70e-39a9-48d0-80b7-48679bcd7850" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "d3c6050c-9b8c-4038-bc33-6acb6cc8b920", "2920f124-e9cd-474a-b186-b5c915216ada" });
        }
    }
}

