using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Task4_3_FixSeedPropertyNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnici_Uloge_UlogaId",
                table: "Korisnici");

            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_Korisnici_KorisnikId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Korisnici_KorisnikId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije");

            migrationBuilder.DropIndex(
                name: "IX_Zaposlenici_Specijalizacija",
                table: "Zaposlenici");

            migrationBuilder.DropIndex(
                name: "IX_Usluge_Naziv",
                table: "Usluge");

            migrationBuilder.DropIndex(
                name: "IX_Proizvodi_Sifra",
                table: "Proizvodi");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Ime",
                table: "Korisnici");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Prezime",
                table: "Korisnici");

            migrationBuilder.DeleteData(
                table: "Gradovi",
                keyColumn: "Id",
                keyValue: 2);

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
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "KategorijeUsluga",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "GradId",
                table: "Korisnici",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Korisnici",
                columns: new[] { "Id", "CreatedAt", "Email", "GradId", "Ime", "IsDeleted", "KorisnickoIme", "PasswordHash", "PasswordSalt", "Prezime", "Status", "Telefon", "UlogaId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@nuaspa.ba", 1, "Admin", false, "admin", "dummy_hash_123", "dummy_salt_123", "NuaSpa", true, "033123456", 1 },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "lana@test.ba", 3, "Lana", false, "lana", "dummy_hash_456", "dummy_salt_456", "Korisnik", true, "061222333", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_GradId",
                table: "Korisnici",
                column: "GradId");

            migrationBuilder.AddForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi",
                column: "DrzavaId",
                principalTable: "Drzave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnici_Gradovi_GradId",
                table: "Korisnici",
                column: "GradId",
                principalTable: "Gradovi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnici_Uloge_UlogaId",
                table: "Korisnici",
                column: "UlogaId",
                principalTable: "Uloge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda",
                column: "ProizvodId",
                principalTable: "Proizvodi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_Korisnici_KorisnikId",
                table: "Recenzije",
                column: "KorisnikId",
                principalTable: "Korisnici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Korisnici_KorisnikId",
                table: "Rezervacije",
                column: "KorisnikId",
                principalTable: "Korisnici",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnici_Gradovi_GradId",
                table: "Korisnici");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnici_Uloge_UlogaId",
                table: "Korisnici");

            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_Korisnici_KorisnikId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Korisnici_KorisnikId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_GradId",
                table: "Korisnici");

            migrationBuilder.DeleteData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "GradId",
                table: "Korisnici");

            migrationBuilder.InsertData(
                table: "Gradovi",
                columns: new[] { "Id", "CreatedAt", "DrzavaId", "IsDeleted", "Naziv", "PostanskiBroj" },
                values: new object[,]
                {
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Mostar", "88000" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Banja Luka", "78000" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Tuzla", "75000" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Zenica", "72000" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Bihać", "77000" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Trebinje", "89000" },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Bijeljina", "76300" },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "Jablanica", "88420" }
                });

            migrationBuilder.InsertData(
                table: "KategorijeUsluga",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "Naziv", "Opis" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Masaže", "Relaksacione i terapeutske masaže" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Tretmani lica", "Njegujući tretmani za kožu" }
                });

            migrationBuilder.InsertData(
                table: "Usluge",
                columns: new[] { "Id", "Cijena", "CreatedAt", "IsDeleted", "KategorijaUslugaId", "Naziv", "Opis", "TrajanjeMinuta" },
                values: new object[,]
                {
                    { 1, 50.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 1, "Švedska masaža", "Klasična masaža tijela", 60 },
                    { 2, 45.00m, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, 2, "Hidratacija lica", "Dubinska hidratacija", 45 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Zaposlenici_Specijalizacija",
                table: "Zaposlenici",
                column: "Specijalizacija");

            migrationBuilder.CreateIndex(
                name: "IX_Usluge_Naziv",
                table: "Usluge",
                column: "Naziv");

            migrationBuilder.CreateIndex(
                name: "IX_Proizvodi_Sifra",
                table: "Proizvodi",
                column: "Sifra",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Ime",
                table: "Korisnici",
                column: "Ime");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Prezime",
                table: "Korisnici",
                column: "Prezime");

            migrationBuilder.AddForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi",
                column: "DrzavaId",
                principalTable: "Drzave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnici_Uloge_UlogaId",
                table: "Korisnici",
                column: "UlogaId",
                principalTable: "Uloge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda",
                column: "ProizvodId",
                principalTable: "Proizvodi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_Korisnici_KorisnikId",
                table: "Recenzije",
                column: "KorisnikId",
                principalTable: "Korisnici",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Korisnici_KorisnikId",
                table: "Rezervacije",
                column: "KorisnikId",
                principalTable: "Korisnici",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
