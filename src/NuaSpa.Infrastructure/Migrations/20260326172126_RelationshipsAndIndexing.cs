using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelationshipsAndIndexing : Migration
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
                name: "FK_Recenzije_Korisnici_KorisnikId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Korisnici_KorisnikId",
                table: "Rezervacije");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnickoIme",
                table: "Korisnici",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Korisnici",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_KorisnickoIme",
                table: "Korisnici",
                column: "KorisnickoIme",
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi");

            migrationBuilder.DropForeignKey(
                name: "FK_Korisnici_Uloge_UlogaId",
                table: "Korisnici");

            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_Korisnici_KorisnikId",
                table: "Recenzije");

            migrationBuilder.DropForeignKey(
                name: "FK_Rezervacije_Korisnici_KorisnikId",
                table: "Rezervacije");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_KorisnickoIme",
                table: "Korisnici");

            migrationBuilder.AlterColumn<string>(
                name: "KorisnickoIme",
                table: "Korisnici",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Korisnici",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Gradovi_Drzave_DrzavaId",
                table: "Gradovi",
                column: "DrzavaId",
                principalTable: "Drzave",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Korisnici_Uloge_UlogaId",
                table: "Korisnici",
                column: "UlogaId",
                principalTable: "Uloge",
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
        }
    }
}
