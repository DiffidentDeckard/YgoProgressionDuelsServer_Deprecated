using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class addrelationbetweenspecialproductandcard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SpecialProductCards_SpecialProductSetName_CardInfoId",
                table: "SpecialProductCards");

            migrationBuilder.DropColumn(
                name: "SpecialProductSetCode",
                table: "SpecialProductCards");

            migrationBuilder.DropColumn(
                name: "SpecialProductSetName",
                table: "SpecialProductCards");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerProductId",
                table: "SpecialProductCards",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_SpecialProductCards_OwnerProductId_CardInfoId",
                table: "SpecialProductCards",
                columns: new[] { "OwnerProductId", "CardInfoId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SpecialProductCards_SpecialProducts_OwnerProductId",
                table: "SpecialProductCards",
                column: "OwnerProductId",
                principalTable: "SpecialProducts",
                principalColumn: "SpecialProductId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpecialProductCards_SpecialProducts_OwnerProductId",
                table: "SpecialProductCards");

            migrationBuilder.DropIndex(
                name: "IX_SpecialProductCards_OwnerProductId_CardInfoId",
                table: "SpecialProductCards");

            migrationBuilder.DropColumn(
                name: "OwnerProductId",
                table: "SpecialProductCards");

            migrationBuilder.AddColumn<string>(
                name: "SpecialProductSetCode",
                table: "SpecialProductCards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialProductSetName",
                table: "SpecialProductCards",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpecialProductCards_SpecialProductSetName_CardInfoId",
                table: "SpecialProductCards",
                columns: new[] { "SpecialProductSetName", "CardInfoId" },
                unique: true,
                filter: "[SpecialProductSetName] IS NOT NULL");
        }
    }
}
