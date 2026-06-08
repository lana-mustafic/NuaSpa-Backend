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

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "48abe503-32c9-465c-86b2-a35c9c295a28");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "64330f20-0592-4421-8c15-b72bc579cd4f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "f9cdb1f6-eeb3-438b-9f16-f8e7682e2ab9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "b4f73608-92f7-40f0-9dcf-d3e54d973776", "1362f6e4-e63f-45a7-bee1-20c4a6fc4677" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "622655d6-a9af-475a-9858-2b0aa1a86af7", "fb4f3f12-a989-4f83-8839-e070a1d1bd4e" });

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

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "94fb707f-2b00-43d7-b717-a51691b34ae9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "dd310383-8f30-4ace-90dd-d42a7dec20e2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "c849a8cb-547a-4d59-956c-fb665e4aecd5");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "b95260b4-464f-4e4f-b532-1e88b6e11a1a", "29b4e549-acac-4768-9a30-ac3fb046a8d7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ecc09def-88ca-4c04-b022-0c980cf494e5", "85a9540e-f36f-41a7-9740-4762e522ff70" });
        }
    }
}
