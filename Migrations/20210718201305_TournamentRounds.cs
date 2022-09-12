using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentRounds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tournament_BoosterPackInfos_BoosterPackInfoId",
                table: "Tournament");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournament_ProgressionSeries_ProgressionSeriesId",
                table: "Tournament");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairing_Duelists_Duelist1Id",
                table: "TournamentPairing");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairing_Duelists_Duelist2Id",
                table: "TournamentPairing");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairing_Tournament_TournamentId",
                table: "TournamentPairing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentPairing",
                table: "TournamentPairing");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament");

            migrationBuilder.DropColumn(
                name: "RoundNumber",
                table: "TournamentPairing");

            migrationBuilder.DropColumn(
                name: "CurrentRoundNumber",
                table: "Tournament");

            migrationBuilder.RenameTable(
                name: "TournamentPairing",
                newName: "TournamentPairings");

            migrationBuilder.RenameTable(
                name: "Tournament",
                newName: "Tournaments");

            migrationBuilder.RenameColumn(
                name: "TournamentId",
                table: "TournamentPairings",
                newName: "TournamentRoundId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairing_TournamentId_Duelist1Id_Duelist2Id",
                table: "TournamentPairings",
                newName: "IX_TournamentPairings_TournamentRoundId_Duelist1Id_Duelist2Id");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairing_Duelist2Id",
                table: "TournamentPairings",
                newName: "IX_TournamentPairings_Duelist2Id");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairing_Duelist1Id",
                table: "TournamentPairings",
                newName: "IX_TournamentPairings_Duelist1Id");

            migrationBuilder.RenameIndex(
                name: "IX_Tournament_ProgressionSeriesId_TournamentNumber",
                table: "Tournaments",
                newName: "IX_Tournaments_ProgressionSeriesId_TournamentNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Tournament_BoosterPackInfoId",
                table: "Tournaments",
                newName: "IX_Tournaments_BoosterPackInfoId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentPairings",
                table: "TournamentPairings",
                column: "PairingId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "TournamentId");

            migrationBuilder.CreateTable(
                name: "TournamentRounds",
                columns: table => new
                {
                    PairingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentRounds", x => x.PairingId);
                    table.ForeignKey(
                        name: "FK_TournamentRounds_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRounds_TournamentId_RoundNumber",
                table: "TournamentRounds",
                columns: new[] { "TournamentId", "RoundNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairings_Duelists_Duelist1Id",
                table: "TournamentPairings",
                column: "Duelist1Id",
                principalTable: "Duelists",
                principalColumn: "DuelistId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairings_Duelists_Duelist2Id",
                table: "TournamentPairings",
                column: "Duelist2Id",
                principalTable: "Duelists",
                principalColumn: "DuelistId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairings_TournamentRounds_TournamentRoundId",
                table: "TournamentPairings",
                column: "TournamentRoundId",
                principalTable: "TournamentRounds",
                principalColumn: "PairingId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_BoosterPackInfos_BoosterPackInfoId",
                table: "Tournaments",
                column: "BoosterPackInfoId",
                principalTable: "BoosterPackInfos",
                principalColumn: "BoosterPackInfoId",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournaments_ProgressionSeries_ProgressionSeriesId",
                table: "Tournaments",
                column: "ProgressionSeriesId",
                principalTable: "ProgressionSeries",
                principalColumn: "ProgressionSeriesId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairings_Duelists_Duelist1Id",
                table: "TournamentPairings");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairings_Duelists_Duelist2Id",
                table: "TournamentPairings");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentPairings_TournamentRounds_TournamentRoundId",
                table: "TournamentPairings");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_BoosterPackInfos_BoosterPackInfoId",
                table: "Tournaments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tournaments_ProgressionSeries_ProgressionSeriesId",
                table: "Tournaments");

            migrationBuilder.DropTable(
                name: "TournamentRounds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentPairings",
                table: "TournamentPairings");

            migrationBuilder.RenameTable(
                name: "Tournaments",
                newName: "Tournament");

            migrationBuilder.RenameTable(
                name: "TournamentPairings",
                newName: "TournamentPairing");

            migrationBuilder.RenameIndex(
                name: "IX_Tournaments_ProgressionSeriesId_TournamentNumber",
                table: "Tournament",
                newName: "IX_Tournament_ProgressionSeriesId_TournamentNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Tournaments_BoosterPackInfoId",
                table: "Tournament",
                newName: "IX_Tournament_BoosterPackInfoId");

            migrationBuilder.RenameColumn(
                name: "TournamentRoundId",
                table: "TournamentPairing",
                newName: "TournamentId");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairings_TournamentRoundId_Duelist1Id_Duelist2Id",
                table: "TournamentPairing",
                newName: "IX_TournamentPairing_TournamentId_Duelist1Id_Duelist2Id");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairings_Duelist2Id",
                table: "TournamentPairing",
                newName: "IX_TournamentPairing_Duelist2Id");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentPairings_Duelist1Id",
                table: "TournamentPairing",
                newName: "IX_TournamentPairing_Duelist1Id");

            migrationBuilder.AddColumn<long>(
                name: "CurrentRoundNumber",
                table: "Tournament",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RoundNumber",
                table: "TournamentPairing",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournament",
                table: "Tournament",
                column: "TournamentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentPairing",
                table: "TournamentPairing",
                column: "PairingId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tournament_BoosterPackInfos_BoosterPackInfoId",
                table: "Tournament",
                column: "BoosterPackInfoId",
                principalTable: "BoosterPackInfos",
                principalColumn: "BoosterPackInfoId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Tournament_ProgressionSeries_ProgressionSeriesId",
                table: "Tournament",
                column: "ProgressionSeriesId",
                principalTable: "ProgressionSeries",
                principalColumn: "ProgressionSeriesId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairing_Duelists_Duelist1Id",
                table: "TournamentPairing",
                column: "Duelist1Id",
                principalTable: "Duelists",
                principalColumn: "DuelistId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairing_Duelists_Duelist2Id",
                table: "TournamentPairing",
                column: "Duelist2Id",
                principalTable: "Duelists",
                principalColumn: "DuelistId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentPairing_Tournament_TournamentId",
                table: "TournamentPairing",
                column: "TournamentId",
                principalTable: "Tournament",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
