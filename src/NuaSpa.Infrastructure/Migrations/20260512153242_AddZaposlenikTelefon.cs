using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddZaposlenikTelefon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telefon",
                table: "Zaposlenici",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telefon",
                table: "Zaposlenici");
        }
    }
}
