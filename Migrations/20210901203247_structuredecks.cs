using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class structuredecks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "StructureDeckPrice",
                table: "ProgressionSeries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsStructureDeck",
                table: "BoosterPackInfos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "StructureDeckCards",
                columns: table => new
                {
                    StructureDeckCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StructureDeckSetCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CardInfoId = table.Column<long>(type: "bigint", nullable: false),
                    CardInfoName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rarity = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructureDeckCards", x => x.StructureDeckCardId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StructureDeckCards_StructureDeckSetCode_CardInfoId",
                table: "StructureDeckCards",
                columns: new[] { "StructureDeckSetCode", "CardInfoId" },
                unique: true,
                filter: "[StructureDeckSetCode] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StructureDeckCards");

            migrationBuilder.DropColumn(
                name: "StructureDeckPrice",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "IsStructureDeck",
                table: "BoosterPackInfos");
        }
    }
}
