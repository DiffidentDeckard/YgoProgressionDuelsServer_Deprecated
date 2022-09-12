using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class RemoveArchetypeColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Archetype",
                table: "CardInfos");

            migrationBuilder.AddColumn<int>(
                name: "BanListFormat",
                table: "ProgressionSeries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BanListFormat",
                table: "ProgressionSeries");

            migrationBuilder.AddColumn<string>(
                name: "Archetype",
                table: "CardInfos",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
