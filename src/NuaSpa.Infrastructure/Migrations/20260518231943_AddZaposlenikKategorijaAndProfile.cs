using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddZaposlenikKategorijaAndProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Specijalizacija",
                table: "Zaposlenici",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "KategorijaUslugaId",
                table: "Zaposlenici",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Jezici",
                table: "Zaposlenici",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Lokacija",
                table: "Zaposlenici",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Obrazovanje",
                table: "Zaposlenici",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zaposlenici_KategorijaUslugaId",
                table: "Zaposlenici",
                column: "KategorijaUslugaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Zaposlenici_KategorijeUsluga_KategorijaUslugaId",
                table: "Zaposlenici",
                column: "KategorijaUslugaId",
                principalTable: "KategorijeUsluga",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Zaposlenici_KategorijeUsluga_KategorijaUslugaId",
                table: "Zaposlenici");

            migrationBuilder.DropIndex(
                name: "IX_Zaposlenici_KategorijaUslugaId",
                table: "Zaposlenici");

            migrationBuilder.DropColumn(
                name: "KategorijaUslugaId",
                table: "Zaposlenici");

            migrationBuilder.DropColumn(
                name: "Jezici",
                table: "Zaposlenici");

            migrationBuilder.DropColumn(
                name: "Lokacija",
                table: "Zaposlenici");

            migrationBuilder.DropColumn(
                name: "Obrazovanje",
                table: "Zaposlenici");

            migrationBuilder.AlterColumn<string>(
                name: "Specijalizacija",
                table: "Zaposlenici",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
