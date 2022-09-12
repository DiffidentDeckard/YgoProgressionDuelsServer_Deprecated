using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class SpecialProducts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowPurchaseSpecialProducts",
                table: "ProgressionSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "SpecialProductPricePerCard",
                table: "ProgressionSeries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "SpecialProductCards",
                columns: table => new
                {
                    SpecialProductCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpecialProductSetName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SpecialProductSetCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CardInfoId = table.Column<long>(type: "bigint", nullable: false),
                    CardInfoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rarity = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialProductCards", x => x.SpecialProductCardId);
                });

            migrationBuilder.CreateTable(
                name: "SpecialProducts",
                columns: table => new
                {
                    SpecialProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SetName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SetCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumCards = table.Column<long>(type: "bigint", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SetInfoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpecialProducts", x => x.SpecialProductId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpecialProductCards_SpecialProductSetName_CardInfoId",
                table: "SpecialProductCards",
                columns: new[] { "SpecialProductSetName", "CardInfoId" },
                unique: true,
                filter: "[SpecialProductSetName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SpecialProducts_SetName",
                table: "SpecialProducts",
                column: "SetName",
                unique: true,
                filter: "[SetName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpecialProductCards");

            migrationBuilder.DropTable(
                name: "SpecialProducts");

            migrationBuilder.DropColumn(
                name: "AllowPurchaseSpecialProducts",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "SpecialProductPricePerCard",
                table: "ProgressionSeries");
        }
    }
}
