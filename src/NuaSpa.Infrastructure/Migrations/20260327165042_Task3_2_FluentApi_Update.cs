using NuaSpa.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Task3_2_FluentApi_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije");

            migrationBuilder.CreateIndex(
                name: "IX_Proizvodi_Sifra",
                table: "Proizvodi",
                column: "Sifra",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda",
                column: "ProizvodId",
                principalTable: "Proizvodi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije");

            migrationBuilder.DropIndex(
                name: "IX_Proizvodi_Sifra",
                table: "Proizvodi");

            migrationBuilder.AddForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda",
                column: "ProizvodId",
                principalTable: "Proizvodi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
