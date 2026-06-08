using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlacanjeRefundAuditAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "RefundedAtUtc",
                table: "Placanja",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefundedByUserId",
                table: "Placanja",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "e92f4df8-134e-403a-9e2c-df302e5ec814");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "1bcdad30-18c5-420e-b27d-37f9a9a1cb49");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "9fb5b6d3-832b-4263-ab5a-bdab8271418a");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "4f0ab62e-907e-43f6-9886-70b24263bda4", "48ce0fba-988b-4cba-88ab-82e52f49d314" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "89cfc5d0-f683-42df-b04d-b0a4a3892df0", "def28134-ca80-4a48-9798-0ef680e76e12" });

            migrationBuilder.CreateIndex(
                name: "IX_Placanja_DatumPlacanja",
                table: "Placanja",
                column: "DatumPlacanja");

            migrationBuilder.CreateIndex(
                name: "IX_Placanja_DatumPlacanja_Status",
                table: "Placanja",
                columns: new[] { "DatumPlacanja", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Placanja_DatumPlacanja",
                table: "Placanja");

            migrationBuilder.DropIndex(
                name: "IX_Placanja_DatumPlacanja_Status",
                table: "Placanja");

            migrationBuilder.DropColumn(
                name: "RefundedAtUtc",
                table: "Placanja");

            migrationBuilder.DropColumn(
                name: "RefundedByUserId",
                table: "Placanja");

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
        }
    }
}
