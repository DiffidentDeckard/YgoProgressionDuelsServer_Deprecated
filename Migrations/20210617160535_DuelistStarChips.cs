using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class DuelistStarChips : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowPurchaseBoosterPacks",
                table: "ProgressionSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "BoosterPackPrice",
                table: "ProgressionSeries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "NumStarChips",
                table: "Duelists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowPurchaseBoosterPacks",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "BoosterPackPrice",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "NumStarChips",
                table: "Duelists");
        }
    }
}
