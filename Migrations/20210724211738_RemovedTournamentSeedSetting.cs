using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class RemovedTournamentSeedSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PairingDuelist_TournamentPairing_PairingId",
                table: "PairingDuelist");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairing_Tournaments_TournamentId",
                table: "TournamentPairing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentPairing",
                table: "TournamentPairing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PairingDuelist",
                table: "PairingDuelist");

            migrationBuilder.DropColumn(
                name: "Seed",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "NextTournamentSeed",
                table: "ProgressionSeries");

            migrationBuilder.RenameTable(
                name: "TournamentPairing",
                newName: "TournamentPairings");

            migrationBuilder.RenameTable(
                name: "PairingDuelist",
                newName: "PairingDuelists");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairing_TournamentId",
                table: "TournamentPairings",
                newName: "IX_TournamentPairings_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_PairingDuelist_PairingId_Username",
                table: "PairingDuelists",
                newName: "IX_PairingDuelists_PairingId_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentPairings",
                table: "TournamentPairings",
                column: "TournamentPairingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PairingDuelists",
                table: "PairingDuelists",
                column: "PairingDuelistId");

            migrationBuilder.AddForeignKey(
                name: "FK_PairingDuelists_TournamentPairings_PairingId",
                table: "PairingDuelists",
                column: "PairingId",
                principalTable: "TournamentPairings",
                principalColumn: "TournamentPairingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairings_Tournaments_TournamentId",
                table: "TournamentPairings",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PairingDuelists_TournamentPairings_PairingId",
                table: "PairingDuelists");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairings_Tournaments_TournamentId",
                table: "TournamentPairings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentPairings",
                table: "TournamentPairings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PairingDuelists",
                table: "PairingDuelists");

            migrationBuilder.RenameTable(
                name: "TournamentPairings",
                newName: "TournamentPairing");

            migrationBuilder.RenameTable(
                name: "PairingDuelists",
                newName: "PairingDuelist");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairings_TournamentId",
                table: "TournamentPairing",
                newName: "IX_TournamentPairing_TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_PairingDuelists_PairingId_Username",
                table: "PairingDuelist",
                newName: "IX_PairingDuelist_PairingId_Username");

            migrationBuilder.AddColumn<int>(
                name: "Seed",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NextTournamentSeed",
                table: "ProgressionSeries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentPairing",
                table: "TournamentPairing",
                column: "TournamentPairingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PairingDuelist",
                table: "PairingDuelist",
                column: "PairingDuelistId");

            migrationBuilder.AddForeignKey(
                name: "FK_PairingDuelist_TournamentPairing_PairingId",
                table: "PairingDuelist",
                column: "PairingId",
                principalTable: "TournamentPairing",
                principalColumn: "TournamentPairingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairing_Tournaments_TournamentId",
                table: "TournamentPairing",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
