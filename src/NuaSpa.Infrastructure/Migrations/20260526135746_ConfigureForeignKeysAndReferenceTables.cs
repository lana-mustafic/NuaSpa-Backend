using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureForeignKeysAndReferenceTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Zaposlenici_ZaposlenikId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi");

            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_AspNetUsers_KorisnikId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_Oprema_SpaCentri_SpaCentarId",
                table: "Oprema");

            migrationBuilder.DropForeignKey(
                name: "FK_Placanja_Rezervacije_RezervacijaId",
                table: "Placanja");

            migrationBuilder.DropForeignKey(
                name: "FK_Prostorije_SpaCentri_SpaCentarId",
                table: "Prostorije");

            migrationBuilder.DropForeignKey(
                name: "FK_RadnaVremena_SpaCentri_SpaCentarId",
                table: "RadnaVremena");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_AspNetUsers_KorisnikId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_Usluge_UslugaId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_AspNetUsers_KorisnikId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Prostorije_ProstorijaId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Usluge_UslugaId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_RezervacijeOprema_Oprema_OpremaId",
                table: "RezervacijeOprema");

            migrationBuilder.DropForeignKey(
                name: "FK_Skladista_Proizvodi_ProizvodId",
                table: "Skladista");

            migrationBuilder.DropForeignKey(
                name: "FK_Usluge_KategorijeUsluga_KategorijaUslugaId",
                table: "Usluge");

            migrationBuilder.AlterTable(
                name: "RezervacijeOprema",
                comment: "Međutabela: rezervacija ↔ oprema");

            migrationBuilder.AlterTable(
                name: "KategorijeUsluga",
                comment: "Referentna tablica: kategorije usluga");

            migrationBuilder.AlterTable(
                name: "Gradovi",
                comment: "Referentna tablica: gradovi");

            migrationBuilder.AlterTable(
                name: "Favoriti",
                comment: "Međutabela: korisnik ↔ omiljena usluga");

            migrationBuilder.AlterTable(
                name: "Drzave",
                comment: "Referentna tablica: države");

            migrationBuilder.AlterTable(
                name: "AspNetRoles",
                comment: "Referentna tablica: uloge (ASP.NET Identity)");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Zaposlenici",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ZaposlenikId",
                table: "Recenzije",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KorisnikAktivnosti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    UslugaId = table.Column<int>(type: "int", nullable: true),
                    KategorijaUslugaId = table.Column<int>(type: "int", nullable: true),
                    SearchTerm = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KorisnikAktivnosti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KorisnikAktivnosti_AspNetUsers_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KorisnikAktivnosti_KategorijeUsluga_KategorijaUslugaId",
                        column: x => x.KategorijaUslugaId,
                        principalTable: "KategorijeUsluga",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_KorisnikAktivnosti_Usluge_UslugaId",
                        column: x => x.UslugaId,
                        principalTable: "Usluge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StaffInvitations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ZaposlenikId = table.Column<int>(type: "int", nullable: false),
                    KorisnikId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByKorisnikId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffInvitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId",
                        column: x => x.CreatedByKorisnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StaffInvitations_AspNetUsers_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffInvitations_Zaposlenici_ZaposlenikId",
                        column: x => x.ZaposlenikId,
                        principalTable: "Zaposlenici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "ca021e71-b5ae-4a2c-8ca8-db893201d5a1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "861a470a-4a25-4ee6-ac0b-2b111db51da6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "bef1c2e1-95b7-414c-9a0f-6477d690bc3a");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "54e2655a-e52d-4fc7-9993-474d1b8ad8ba", "aa4e9496-976a-40dc-b312-6810e190b5a3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "789851c9-229f-48f8-b2f1-15f29546f231", "acdaab69-644f-46d7-9b93-a74cb22951e8" });

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM [KategorijeUsluga] WHERE [Id] = 1)
                    INSERT INTO [KategorijeUsluga] ([Id], [Naziv], [Opis], [CreatedAt], [IsDeleted])
                    VALUES (1, N'Massage', N'Masaže i relaks tretmani', '2026-01-01', 0);
                IF NOT EXISTS (SELECT 1 FROM [KategorijeUsluga] WHERE [Id] = 2)
                    INSERT INTO [KategorijeUsluga] ([Id], [Naziv], [Opis], [CreatedAt], [IsDeleted])
                    VALUES (2, N'Facial', N'Tretmani lica', '2026-01-01', 0);
                IF NOT EXISTS (SELECT 1 FROM [KategorijeUsluga] WHERE [Id] = 3)
                    INSERT INTO [KategorijeUsluga] ([Id], [Naziv], [Opis], [CreatedAt], [IsDeleted])
                    VALUES (3, N'Body', N'Tretmani tijela', '2026-01-01', 0);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_ZaposlenikId",
                table: "Recenzije",
                column: "ZaposlenikId");

            migrationBuilder.CreateIndex(
                name: "IX_KorisnikAktivnosti_KategorijaUslugaId",
                table: "KorisnikAktivnosti",
                column: "KategorijaUslugaId");

            migrationBuilder.CreateIndex(
                name: "IX_KorisnikAktivnosti_KorisnikId_CreatedAt",
                table: "KorisnikAktivnosti",
                columns: new[] { "KorisnikId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_KorisnikAktivnosti_UslugaId",
                table: "KorisnikAktivnosti",
                column: "UslugaId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_CreatedByKorisnikId",
                table: "StaffInvitations",
                column: "CreatedByKorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_KorisnikId",
                table: "StaffInvitations",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_TokenHash",
                table: "StaffInvitations",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_StaffInvitations_ZaposlenikId_AcceptedAt",
                table: "StaffInvitations",
                columns: new[] { "ZaposlenikId", "AcceptedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Zaposlenici_ZaposlenikId",
                table: "AspNetUsers",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi",
                column: "DrzavaId",
                principalTable: "Drzave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NarudzbeProizvoda_AspNetUsers_KorisnikId",
                table: "NarudzbeProizvoda",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
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
                name: "FK_Oprema_SpaCentri_SpaCentarId",
                table: "Oprema",
                column: "SpaCentarId",
                principalTable: "SpaCentri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Placanja_Rezervacije_RezervacijaId",
                table: "Placanja",
                column: "RezervacijaId",
                principalTable: "Rezervacije",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Prostorije_SpaCentri_SpaCentarId",
                table: "Prostorije",
                column: "SpaCentarId",
                principalTable: "SpaCentri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RadnaVremena_SpaCentri_SpaCentarId",
                table: "RadnaVremena",
                column: "SpaCentarId",
                principalTable: "SpaCentri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_AspNetUsers_KorisnikId",
                table: "Recenzije",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_Usluge_UslugaId",
                table: "Recenzije",
                column: "UslugaId",
                principalTable: "Usluge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_Zaposlenici_ZaposlenikId",
                table: "Recenzije",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_AspNetUsers_KorisnikId",
                table: "Rezervacije",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Prostorije_ProstorijaId",
                table: "Rezervacije",
                column: "ProstorijaId",
                principalTable: "Prostorije",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Usluge_UslugaId",
                table: "Rezervacije",
                column: "UslugaId",
                principalTable: "Usluge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RezervacijeOprema_Oprema_OpremaId",
                table: "RezervacijeOprema",
                column: "OpremaId",
                principalTable: "Oprema",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Skladista_Proizvodi_ProizvodId",
                table: "Skladista",
                column: "ProizvodId",
                principalTable: "Proizvodi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Usluge_KategorijeUsluga_KategorijaUslugaId",
                table: "Usluge",
                column: "KategorijaUslugaId",
                principalTable: "KategorijeUsluga",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Zaposlenici_ZaposlenikId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi");

            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_AspNetUsers_KorisnikId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_NarudzbeProizvoda_Proizvodi_ProizvodId",
                table: "NarudzbeProizvoda");

            migrationBuilder.DropForeignKey(
                name: "FK_Oprema_SpaCentri_SpaCentarId",
                table: "Oprema");

            migrationBuilder.DropForeignKey(
                name: "FK_Placanja_Rezervacije_RezervacijaId",
                table: "Placanja");

            migrationBuilder.DropForeignKey(
                name: "FK_Prostorije_SpaCentri_SpaCentarId",
                table: "Prostorije");

            migrationBuilder.DropForeignKey(
                name: "FK_RadnaVremena_SpaCentri_SpaCentarId",
                table: "RadnaVremena");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_AspNetUsers_KorisnikId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_Usluge_UslugaId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_Zaposlenici_ZaposlenikId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_AspNetUsers_KorisnikId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Prostorije_ProstorijaId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Usluge_UslugaId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije");

            migrationBuilder.DropForeignKey(
                name: "FK_RezervacijeOprema_Oprema_OpremaId",
                table: "RezervacijeOprema");

            migrationBuilder.DropForeignKey(
                name: "FK_Skladista_Proizvodi_ProizvodId",
                table: "Skladista");

            migrationBuilder.DropForeignKey(
                name: "FK_Usluge_KategorijeUsluga_KategorijaUslugaId",
                table: "Usluge");

            migrationBuilder.DropTable(
                name: "KorisnikAktivnosti");

            migrationBuilder.DropTable(
                name: "StaffInvitations");

            migrationBuilder.DropIndex(
                name: "IX_Recenzije_ZaposlenikId",
                table: "Recenzije");

            migrationBuilder.DeleteData(
                table: "KategorijeUsluga",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "KategorijeUsluga",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "KategorijeUsluga",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Zaposlenici");

            migrationBuilder.DropColumn(
                name: "ZaposlenikId",
                table: "Recenzije");

            migrationBuilder.AlterTable(
                name: "RezervacijeOprema",
                oldComment: "Međutabela: rezervacija ↔ oprema");

            migrationBuilder.AlterTable(
                name: "KategorijeUsluga",
                oldComment: "Referentna tablica: kategorije usluga");

            migrationBuilder.AlterTable(
                name: "Gradovi",
                oldComment: "Referentna tablica: gradovi");

            migrationBuilder.AlterTable(
                name: "Favoriti",
                oldComment: "Međutabela: korisnik ↔ omiljena usluga");

            migrationBuilder.AlterTable(
                name: "Drzave",
                oldComment: "Referentna tablica: države");

            migrationBuilder.AlterTable(
                name: "AspNetRoles",
                oldComment: "Referentna tablica: uloge (ASP.NET Identity)");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "6549b09e-691f-441a-8b62-b72113d2dc9c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "0d6e4ad7-8421-4f1c-bd6a-847d07814c1c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "49ac1959-8ee5-481c-b913-4b590c8612d0");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "98a7d6b1-bf2b-4b12-9f7f-7139a9d83e12", "195ef48f-59bf-4e35-8215-43d614349361" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "54edfcf9-630f-4186-9c61-6b1f8e9e089c", "fb62fe66-2693-41e7-8431-50334409ea75" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Zaposlenici_ZaposlenikId",
                table: "AspNetUsers",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi",
                column: "DrzavaId",
                principalTable: "Drzave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_NarudzbeProizvoda_AspNetUsers_KorisnikId",
                table: "NarudzbeProizvoda",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
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
                name: "FK_Oprema_SpaCentri_SpaCentarId",
                table: "Oprema",
                column: "SpaCentarId",
                principalTable: "SpaCentri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Placanja_Rezervacije_RezervacijaId",
                table: "Placanja",
                column: "RezervacijaId",
                principalTable: "Rezervacije",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Prostorije_SpaCentri_SpaCentarId",
                table: "Prostorije",
                column: "SpaCentarId",
                principalTable: "SpaCentri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RadnaVremena_SpaCentri_SpaCentarId",
                table: "RadnaVremena",
                column: "SpaCentarId",
                principalTable: "SpaCentri",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_AspNetUsers_KorisnikId",
                table: "Recenzije",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_Usluge_UslugaId",
                table: "Recenzije",
                column: "UslugaId",
                principalTable: "Usluge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_AspNetUsers_KorisnikId",
                table: "Rezervacije",
                column: "KorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Prostorije_ProstorijaId",
                table: "Rezervacije",
                column: "ProstorijaId",
                principalTable: "Prostorije",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Usluge_UslugaId",
                table: "Rezervacije",
                column: "UslugaId",
                principalTable: "Usluge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rezervacije_Zaposlenici_ZaposlenikId",
                table: "Rezervacije",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RezervacijeOprema_Oprema_OpremaId",
                table: "RezervacijeOprema",
                column: "OpremaId",
                principalTable: "Oprema",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Skladista_Proizvodi_ProizvodId",
                table: "Skladista",
                column: "ProizvodId",
                principalTable: "Proizvodi",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Usluge_KategorijeUsluga_KategorijaUslugaId",
                table: "Usluge",
                column: "KategorijaUslugaId",
                principalTable: "KategorijeUsluga",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
