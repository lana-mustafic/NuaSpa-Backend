using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Task4_1_SeedDataExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Gradovi",
                columns: new[] { "Id", "CreatedAt", "DrzavaId", "IsDeleted", "Naziv", "PostanskiBroj" },
                values: new object[,]
                {
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Konjic", "88400" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Banja Luka", "78000" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Tuzla", "75000" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Zenica", "72000" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Bihać", "77000" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Trebinje", "89000" },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Bijeljina", "76300" },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Jablanica", "88420" }
                });

            migrationBuilder.UpdateData(
                table: "Uloge",
                keyColumn: "Id",
                keyValue: 2,
                column: "Naziv",
                value: "Klijent");

            migrationBuilder.UpdateData(
                table: "Uloge",
                keyColumn: "Id",
                keyValue: 3,
                column: "Naziv",
                value: "Zaposlenik");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.UpdateData(
                table: "Uloge",
                keyColumn: "Id",
                keyValue: 2,
                column: "Naziv",
                value: "Desktop");

            migrationBuilder.UpdateData(
                table: "Uloge",
                keyColumn: "Id",
                keyValue: 3,
                column: "Naziv",
                value: "Mobile");
        }
    }
}
