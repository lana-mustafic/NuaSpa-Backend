using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKorisnikNapomenaZaTerapeuta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NapomenaZaTerapeuta",
                table: "AspNetUsers",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "9b147895-495e-47cf-bafe-874c05f51b30");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "f1619414-0f1d-42da-bda3-5b084f6fd155");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "a64c6615-6b0d-411b-a1bf-024a0dae49f9");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "NapomenaZaTerapeuta", "SecurityStamp" },
                values: new object[] { "1d7c5c89-130d-432e-8cd9-d09b38d05ee9", null, "8fb863c4-c8cb-4355-a842-8ba36dc97585" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "NapomenaZaTerapeuta", "SecurityStamp" },
                values: new object[] { "9133d987-4cfc-4069-90c6-0a63645939ca", null, "300f2586-1d47-43c8-9698-1ed339ac7846" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NapomenaZaTerapeuta",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "fa094f92-ebde-4bb2-8d98-d733d4ab4ee0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "c974e00a-d76b-4b45-a3ee-d7158dbe7de9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "912c0ff7-1e52-4bea-be2e-8dbfe1d3fe1f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6649fd8b-61c7-4f7a-b1dc-9112ddd609c9", "fdefcf09-c18c-4481-96a3-33ec97f7f919" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6a9bc4c9-fe43-4de5-85d4-56c3f4d7c89b", "61ffcf57-b607-4ec6-91e3-e1ca6f67215d" });
        }
    }
}
