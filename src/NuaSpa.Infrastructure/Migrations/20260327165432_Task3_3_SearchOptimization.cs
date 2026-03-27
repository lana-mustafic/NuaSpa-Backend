using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Task3_3_SearchOptimization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Zaposlenici_Specijalizacija",
                table: "Zaposlenici",
                column: "Specijalizacija");

            migrationBuilder.CreateIndex(
                name: "IX_Usluge_Naziv",
                table: "Usluge",
                column: "Naziv");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Ime",
                table: "Korisnici",
                column: "Ime");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Prezime",
                table: "Korisnici",
                column: "Prezime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Zaposlenici_Specijalizacija",
                table: "Zaposlenici");

            migrationBuilder.DropIndex(
                name: "IX_Usluge_Naziv",
                table: "Usluge");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Ime",
                table: "Korisnici");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Prezime",
                table: "Korisnici");
        }
    }
}
