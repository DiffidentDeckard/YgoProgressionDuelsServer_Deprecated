using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentDuelistScoreNameChange2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastOpponentDefeatedScore",
                table: "TournamentDuelists",
                newName: "OpponentsOpponentsDefeated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OpponentsOpponentsDefeated",
                table: "TournamentDuelists",
                newName: "LastOpponentDefeatedScore");
        }
    }
}
