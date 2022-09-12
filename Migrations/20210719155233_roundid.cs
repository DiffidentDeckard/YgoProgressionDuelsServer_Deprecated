using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class roundid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairings_TournamentRounds_TournamentRoundId",
                table: "TournamentPairings");

            migrationBuilder.RenameColumn(
                name: "PairingId",
                table: "TournamentRounds",
                newName: "RoundId");

            migrationBuilder.RenameColumn(
                name: "TournamentRoundId",
                table: "TournamentPairings",
                newName: "RoundId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairings_TournamentRoundId_Duelist1Id_Duelist2Id",
                table: "TournamentPairings",
                newName: "IX_TournamentPairings_RoundId_Duelist1Id_Duelist2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairings_TournamentRounds_RoundId",
                table: "TournamentPairings",
                column: "RoundId",
                principalTable: "TournamentRounds",
                principalColumn: "RoundId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairings_TournamentRounds_RoundId",
                table: "TournamentPairings");

            migrationBuilder.RenameColumn(
                name: "RoundId",
                table: "TournamentRounds",
                newName: "PairingId");

            migrationBuilder.RenameColumn(
                name: "RoundId",
                table: "TournamentPairings",
                newName: "TournamentRoundId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairings_RoundId_Duelist1Id_Duelist2Id",
                table: "TournamentPairings",
                newName: "IX_TournamentPairings_TournamentRoundId_Duelist1Id_Duelist2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairings_TournamentRounds_TournamentRoundId",
                table: "TournamentPairings",
                column: "TournamentRoundId",
                principalTable: "TournamentRounds",
                principalColumn: "PairingId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
