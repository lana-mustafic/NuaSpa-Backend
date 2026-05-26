using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NuaSpa.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedJwt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId",
                table: "StaffInvitations");

            migrationBuilder.CreateTable(
                name: "RevokedJwts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Jti = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevokedJwts", x => x.Id);
                },
                comment: "Opozvani JWT tokeni (logout)");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "1beb32a9-e4e3-4d05-80b4-0d651fcbea98");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "017b1c6d-fbae-4f4f-b1ca-ec829b28e63a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "789ae94f-669d-4b2a-92af-c9e98ad45b1c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "66c1a2f6-2f34-40f5-9881-066d5d069a09", "47abb512-5f38-45b9-a3e2-a96742d2a774" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "7c074346-0812-414c-b3c9-4f638676abad", "d10ddd65-341e-448b-9b95-18b545c423b1" });

            migrationBuilder.CreateIndex(
                name: "IX_RevokedJwts_ExpiresAtUtc",
                table: "RevokedJwts",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RevokedJwts_Jti",
                table: "RevokedJwts",
                column: "Jti",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId",
                table: "StaffInvitations",
                column: "CreatedByKorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId",
                table: "StaffInvitations");

            migrationBuilder.DropTable(
                name: "RevokedJwts");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "ca021e71-b5ae-4a2c-8ca8-db893201d5a1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "861a470a-4a25-4ee6-ac0b-2b111db51da6");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "bef1c2e1-95b7-414c-9a0f-6477d690bc3a");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "54e2655a-e52d-4fc7-9993-474d1b8ad8ba", "aa4e9496-976a-40dc-b312-6810e190b5a3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "789851c9-229f-48f8-b2f1-15f29546f231", "acdaab69-644f-46d7-9b93-a74cb22951e8" });

            migrationBuilder.AddForeignKey(
                name: "FK_StaffInvitations_AspNetUsers_CreatedByKorisnikId",
                table: "StaffInvitations",
                column: "CreatedByKorisnikId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
