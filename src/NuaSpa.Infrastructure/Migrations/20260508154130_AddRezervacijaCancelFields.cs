using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRezervacijaCancelFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOtkazana",
                table: "Rezervacije",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OtkazanaAt",
                table: "Rezervacije",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazlogOtkaza",
                table: "Rezervacije",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "8a375e95-21da-410a-b55d-34129d9684c2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "bfd2a069-0236-4c14-aca7-0ce04c12ad97");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "8c343046-d84b-4b88-a3fb-fac67323e843");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a730d1d2-d5cd-49ea-9675-96eddc3b40ee", "2daa28c1-e051-4456-8f35-55170d7dad22" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "fca6684d-3858-481a-bfe3-f2677291c586", "09efd599-0b2c-4718-b8cc-a914327440fa" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOtkazana",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "OtkazanaAt",
                table: "Rezervacije");

            migrationBuilder.DropColumn(
                name: "RazlogOtkaza",
                table: "Rezervacije");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "e70170b2-6ec7-4f27-bb3e-86bccb7f8b7e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "66908310-f54d-4da6-a191-a354a95bd9d3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "0b9f5afc-fa5f-4a6f-82bb-4d52ba088dd3");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2fea3636-6a56-4cdc-bfb4-5396199ad94e", "4a804082-4b3f-480e-bffd-f41dbd1aab44" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "561ee2d5-2291-4d74-8305-1e31226700e0", "db0397cc-9d49-435f-8aa0-77ebf71c222c" });
        }
    }
}
