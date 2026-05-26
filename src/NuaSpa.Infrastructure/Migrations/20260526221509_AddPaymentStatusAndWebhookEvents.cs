using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStatusAndWebhookEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatumZavrsetka",
                table: "Placanja",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NaplaceniIznos",
                table: "Placanja",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Placanja",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
UPDATE p
SET p.Status = 1,
    p.DatumZavrsetka = COALESCE(p.DatumZavrsetka, p.DatumPlacanja),
    p.NaplaceniIznos = COALESCE(p.NaplaceniIznos, p.Iznos)
FROM Placanja p
INNER JOIN Rezervacije r ON r.Id = p.RezervacijaId
WHERE r.IsPlacena = 1;
");

            migrationBuilder.AddColumn<string>(
                name: "StripeRefundId",
                table: "Placanja",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StripeWebhookEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StripeEventId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeWebhookEvents", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "703a2de1-0231-4a2f-961c-f0f2e4be5025");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "f1c8a2df-9ac0-40d4-9c28-c66f39d3f57e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "09ee6807-b121-4a8a-82b6-6065a86de6ec");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "a1fb69f7-dc1b-4d13-bcf1-8f0ea9d5b85d", "502d123f-c708-4f4d-a6a0-ccb74e5b58ce" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "69ff5cc0-f86f-491b-8db1-31b7aaf208bb", "4df108b2-8873-4d81-882d-7c852726c2b7" });

            migrationBuilder.Sql(@"
;WITH ranked AS (
    SELECT Id,
           ROW_NUMBER() OVER (PARTITION BY TransakcijskiBroj ORDER BY Id DESC) AS rn
    FROM Placanja
)
UPDATE p
SET p.Status = 3
FROM Placanja p
INNER JOIN ranked r ON r.Id = p.Id
WHERE r.rn > 1;
");

            migrationBuilder.CreateIndex(
                name: "IX_Placanja_TransakcijskiBroj",
                table: "Placanja",
                column: "TransakcijskiBroj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StripeWebhookEvents_StripeEventId",
                table: "StripeWebhookEvents",
                column: "StripeEventId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StripeWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_Placanja_TransakcijskiBroj",
                table: "Placanja");

            migrationBuilder.DropColumn(
                name: "DatumZavrsetka",
                table: "Placanja");

            migrationBuilder.DropColumn(
                name: "NaplaceniIznos",
                table: "Placanja");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Placanja");

            migrationBuilder.DropColumn(
                name: "StripeRefundId",
                table: "Placanja");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "0964540f-3f7f-4787-a343-b8f3c55187cc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "8822c881-ab03-4f04-b0c0-9f3149ad9b8a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "79bae97a-207b-4e8e-a767-779b2f42ecc8");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f40fcbab-af3d-44d9-9531-8c0deb176ba0", "0689e144-6636-4436-8b06-57b8efb73820" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "d53171c5-4ba2-4d72-bac3-219836142f7e", "f1eb1c70-3b47-4a93-85b8-aa3f1c123039" });
        }
    }
}
