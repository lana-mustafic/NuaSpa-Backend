using NuaSpa.Domain;
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Task4_2_FullSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "KategorijeUsluga",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Naziv", "Opis" },
                values: new object[] { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Tretmani lica", "Njegujući tretmani za kožu" });

            migrationBuilder.InsertData(
                table: "Usluge",
                columns: new[] { "Id", "Cijena", "CreatedAt", "IsDeleted", "KategorijaUslugaId", "Naziv", "Opis", "TrajanjeMinuta" },
                values: new object[,]
                {
                    { 1, 50.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, "Švedska masaža", "Klasična masaža tijela", 60 },
                    { 2, 45.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, "Hidratacija lica", "Dubinska hidratacija", 45 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usluge",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usluge",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "KategorijeUsluga",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
