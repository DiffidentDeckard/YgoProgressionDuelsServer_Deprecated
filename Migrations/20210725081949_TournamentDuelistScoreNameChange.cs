using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentDuelistScoreNameChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpponentScore",
                table: "TournamentDuelists",
                newName: "OpponentsDefeated");

            migrationBuilder.RenameColumn(
                name: "OpponentOpponentScore",
                table: "TournamentDuelists",
                newName: "LastOpponentDefeatedScore");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpponentsDefeated",
                table: "TournamentDuelists",
                newName: "OpponentScore");

            migrationBuilder.RenameColumn(
                name: "LastOpponentDefeatedScore",
                table: "TournamentDuelists",
                newName: "OpponentOpponentScore");
        }
    }
}
