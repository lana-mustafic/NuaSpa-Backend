using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropUnusedInventoryCatalogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NarudzbeProizvoda");

            migrationBuilder.DropTable(
                name: "Popusti");

            migrationBuilder.DropTable(
                name: "Skladista");

            migrationBuilder.DropTable(
                name: "Proizvodi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Popusti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAktivan = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Procenat = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    VrijediDo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VrijediOd = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Popusti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proizvodi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cijena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Sifra = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Slika = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proizvodi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NarudzbeProizvoda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    ProizvodId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Kolicina = table.Column<int>(type: "int", nullable: false),
                    UkupnaCijena = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NarudzbeProizvoda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NarudzbeProizvoda_AspNetUsers_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                        column: x => x.ProizvodId,
                        principalTable: "Proizvodi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Skladista",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProizvodId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    KolicinaNaStanju = table.Column<int>(type: "int", nullable: false),
                    Lokacija = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skladista", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skladista_Proizvodi_ProizvodId",
                        column: x => x.ProizvodId,
                        principalTable: "Proizvodi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NarudzbeProizvoda_KorisnikId",
                table: "NarudzbeProizvoda",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_NarudzbeProizvoda_ProizvodId",
                table: "NarudzbeProizvoda",
                column: "ProizvodId");

            migrationBuilder.CreateIndex(
                name: "IX_Skladista_ProizvodId",
                table: "Skladista",
                column: "ProizvodId");
        }
    }
}
