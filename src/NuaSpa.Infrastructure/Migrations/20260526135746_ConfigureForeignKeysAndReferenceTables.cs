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

            // Idempotentno: kolone/tablice mogu već postojati iz ranijeg Ensure SQL-a.
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('dbo.Zaposlenici', 'Status') IS NULL
                    ALTER TABLE [Zaposlenici] ADD [Status] int NOT NULL
                        CONSTRAINT [DF_Zaposlenici_Status] DEFAULT 0;

                IF COL_LENGTH('dbo.Recenzije', 'ZaposlenikId') IS NULL
                    ALTER TABLE [Recenzije] ADD [ZaposlenikId] int NULL;

                IF OBJECT_ID(N'dbo.KorisnikAktivnosti', N'U') IS NULL
                BEGIN
                    CREATE TABLE [KorisnikAktivnosti] (
                        [Id] int NOT NULL IDENTITY,
                        [KorisnikId] int NOT NULL,
                        [Tip] int NOT NULL,
                        [UslugaId] int NULL,
                        [KategorijaUslugaId] int NULL,
                        [SearchTerm] nvarchar(200) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [IsDeleted] bit NOT NULL CONSTRAINT [DF_KorisnikAktivnosti_IsDeleted] DEFAULT 0,
                        CONSTRAINT [PK_KorisnikAktivnosti] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_KorisnikAktivnosti_AspNetUsers_KorisnikId]
                            FOREIGN KEY ([KorisnikId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_KorisnikAktivnosti_KategorijeUsluga_KategorijaUslugaId]
                            FOREIGN KEY ([KategorijaUslugaId]) REFERENCES [KategorijeUsluga]([Id]) ON DELETE SET NULL,
                        CONSTRAINT [FK_KorisnikAktivnosti_Usluge_UslugaId]
                            FOREIGN KEY ([UslugaId]) REFERENCES [Usluge]([Id]) ON DELETE SET NULL
                    );
                END
                ELSE
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.foreign_keys
                        WHERE name = 'FK_KorisnikAktivnosti_KategorijeUsluga_KategorijaUslugaId')
                    BEGIN
                        ALTER TABLE [KorisnikAktivnosti] ADD CONSTRAINT [FK_KorisnikAktivnosti_KategorijeUsluga_KategorijaUslugaId]
                            FOREIGN KEY ([KategorijaUslugaId]) REFERENCES [KategorijeUsluga]([Id]) ON DELETE SET NULL;
                    END;
                END;

                IF OBJECT_ID(N'dbo.StaffInvitations', N'U') IS NULL
                BEGIN
                    CREATE TABLE [StaffInvitations] (
                        [Id] int NOT NULL IDENTITY,
                        [ZaposlenikId] int NOT NULL,
                        [KorisnikId] int NOT NULL,
                        [Email] nvarchar(256) NOT NULL,
                        [TokenHash] nvarchar(64) NOT NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [ExpiresAt] datetime2 NOT NULL,
                        [AcceptedAt] datetime2 NULL,
                        [CreatedByKorisnikId] int NULL,
                        CONSTRAINT [PK_StaffInvitations] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId]
                            FOREIGN KEY ([CreatedByKorisnikId]) REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION,
                        CONSTRAINT [FK_StaffInvitations_AspNetUsers_KorisnikId]
                            FOREIGN KEY ([KorisnikId]) REFERENCES [AspNetUsers]([Id]) ON DELETE CASCADE,
                        CONSTRAINT [FK_StaffInvitations_Zaposlenici_ZaposlenikId]
                            FOREIGN KEY ([ZaposlenikId]) REFERENCES [Zaposlenici]([Id]) ON DELETE CASCADE
                    );
                END
                ELSE IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId')
                BEGIN
                    ALTER TABLE [StaffInvitations] ADD CONSTRAINT [FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId]
                        FOREIGN KEY ([CreatedByKorisnikId]) REFERENCES [AspNetUsers]([Id]) ON DELETE NO ACTION;
                END;
                """);

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
                SET IDENTITY_INSERT [KategorijeUsluga] ON;

                IF NOT EXISTS (SELECT 1 FROM [KategorijeUsluga] WHERE [Id] = 1)
                    INSERT INTO [KategorijeUsluga] ([Id], [Naziv], [Opis], [CreatedAt], [IsDeleted])
                    VALUES (1, N'Massage', N'Masaže i relaks tretmani', '2026-01-01', 0);
                IF NOT EXISTS (SELECT 1 FROM [KategorijeUsluga] WHERE [Id] = 2)
                    INSERT INTO [KategorijeUsluga] ([Id], [Naziv], [Opis], [CreatedAt], [IsDeleted])
                    VALUES (2, N'Facial', N'Tretmani lica', '2026-01-01', 0);
                IF NOT EXISTS (SELECT 1 FROM [KategorijeUsluga] WHERE [Id] = 3)
                    INSERT INTO [KategorijeUsluga] ([Id], [Naziv], [Opis], [CreatedAt], [IsDeleted])
                    VALUES (3, N'Body', N'Tretmani tijela', '2026-01-01', 0);

                SET IDENTITY_INSERT [KategorijeUsluga] OFF;
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Recenzije_ZaposlenikId' AND object_id = OBJECT_ID('dbo.Recenzije'))
                    CREATE INDEX [IX_Recenzije_ZaposlenikId] ON [Recenzije]([ZaposlenikId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KorisnikAktivnosti_KategorijaUslugaId' AND object_id = OBJECT_ID('dbo.KorisnikAktivnosti'))
                    CREATE INDEX [IX_KorisnikAktivnosti_KategorijaUslugaId] ON [KorisnikAktivnosti]([KategorijaUslugaId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KorisnikAktivnosti_KorisnikId_CreatedAt' AND object_id = OBJECT_ID('dbo.KorisnikAktivnosti'))
                    CREATE INDEX [IX_KorisnikAktivnosti_KorisnikId_CreatedAt] ON [KorisnikAktivnosti]([KorisnikId], [CreatedAt]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_KorisnikAktivnosti_UslugaId' AND object_id = OBJECT_ID('dbo.KorisnikAktivnosti'))
                    CREATE INDEX [IX_KorisnikAktivnosti_UslugaId] ON [KorisnikAktivnosti]([UslugaId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffInvitations_CreatedByKorisnikId' AND object_id = OBJECT_ID('dbo.StaffInvitations'))
                    CREATE INDEX [IX_StaffInvitations_CreatedByKorisnikId] ON [StaffInvitations]([CreatedByKorisnikId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffInvitations_KorisnikId' AND object_id = OBJECT_ID('dbo.StaffInvitations'))
                    CREATE INDEX [IX_StaffInvitations_KorisnikId] ON [StaffInvitations]([KorisnikId]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffInvitations_TokenHash' AND object_id = OBJECT_ID('dbo.StaffInvitations'))
                    CREATE INDEX [IX_StaffInvitations_TokenHash] ON [StaffInvitations]([TokenHash]);

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_StaffInvitations_ZaposlenikId_AcceptedAt' AND object_id = OBJECT_ID('dbo.StaffInvitations'))
                    CREATE INDEX [IX_StaffInvitations_ZaposlenikId_AcceptedAt] ON [StaffInvitations]([ZaposlenikId], [AcceptedAt]);
                """);

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

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Recenzije_Zaposlenici_ZaposlenikId')
                    ALTER TABLE [Recenzije] DROP CONSTRAINT [FK_Recenzije_Zaposlenici_ZaposlenikId];

                ALTER TABLE [Recenzije] ADD CONSTRAINT [FK_Recenzije_Zaposlenici_ZaposlenikId]
                    FOREIGN KEY ([ZaposlenikId]) REFERENCES [Zaposlenici]([Id]) ON DELETE SET NULL;
                """);

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
