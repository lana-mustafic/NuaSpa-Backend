using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKorisnikZaposlenikLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ZaposlenikId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "8669e657-7408-4382-9638-c87ad7b1025e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "c7bc57e9-93db-4a17-9539-7af8cc371a18");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "e08d3538-201d-4a68-9fca-6ca5253d97f8");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp", "ZaposlenikId" },
                values: new object[] { "975efd97-d05f-4a45-8481-3979d0753bde", "7da123bc-6408-414d-8c07-20bd34098620", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp", "ZaposlenikId" },
                values: new object[] { "ef3d3ca4-0447-4980-b843-ff07046c49b8", "d360531e-ec33-415e-b940-492e6a179a3a", null });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ZaposlenikId",
                table: "AspNetUsers",
                column: "ZaposlenikId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Zaposlenici_ZaposlenikId",
                table: "AspNetUsers",
                column: "ZaposlenikId",
                principalTable: "Zaposlenici",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Zaposlenici_ZaposlenikId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ZaposlenikId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ZaposlenikId",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "b1532409-4d7a-42e9-9254-f59c2efd6173");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "a431a21c-e0d9-4b8d-9b59-f907a285c366");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "4c479290-7544-4a7b-acbd-25b45847f8e9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "32d8742f-0161-48cc-a719-ca969b08e425", "bc68eed8-147b-4614-b1a1-17cc3528f085" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "d83f4739-b879-4e56-aba9-5a44c76d34c9", "8a006b88-3127-4c29-b324-a0aefdbc18c7" });
        }
    }
}
