using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecenzijaRezervacijaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RezervacijaId",
                table: "Recenzije",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recenzije_RezervacijaId",
                table: "Recenzije",
                column: "RezervacijaId",
                unique: true,
                filter: "[RezervacijaId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Recenzije_Rezervacije_RezervacijaId",
                table: "Recenzije",
                column: "RezervacijaId",
                principalTable: "Rezervacije",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recenzije_Rezervacije_RezervacijaId",
                table: "Recenzije");

            migrationBuilder.DropIndex(
                name: "IX_Recenzije_RezervacijaId",
                table: "Recenzije");

            migrationBuilder.DropColumn(
                name: "RezervacijaId",
                table: "Recenzije");
        }
    }
}
