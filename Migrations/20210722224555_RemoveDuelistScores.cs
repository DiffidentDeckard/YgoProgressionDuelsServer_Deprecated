using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class RemoveDuelistScores : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentTournamentScore",
                table: "Duelists");

            migrationBuilder.DropColumn(
                name: "TotalTournamentsScore",
                table: "Duelists");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CurrentTournamentScore",
                table: "Duelists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "TotalTournamentsScore",
                table: "Duelists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
