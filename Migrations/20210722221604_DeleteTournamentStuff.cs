using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class DeleteTournamentStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentPairings");

            migrationBuilder.DropTable(
                name: "TournamentRounds");

            migrationBuilder.DropTable(
                name: "Tournaments");

            migrationBuilder.AddColumn<int>(
                name: "NextTournamentBye",
                table: "ProgressionSeries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NextTournamentSeed",
                table: "ProgressionSeries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NextTournamentStructure",
                table: "ProgressionSeries",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextTournamentBye",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "NextTournamentSeed",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "NextTournamentStructure",
                table: "ProgressionSeries");

            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoosterPackInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateStarted = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProgressionSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.TournamentId);
                    table.ForeignKey(
                        name: "FK_Tournaments_BoosterPackInfos_BoosterPackInfoId",
                        column: x => x.BoosterPackInfoId,
                        principalTable: "BoosterPackInfos",
                        principalColumn: "BoosterPackInfoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tournaments_ProgressionSeries_ProgressionSeriesId",
                        column: x => x.ProgressionSeriesId,
                        principalTable: "ProgressionSeries",
                        principalColumn: "ProgressionSeriesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentRounds",
                columns: table => new
                {
                    RoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundNumber = table.Column<long>(type: "bigint", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentRounds", x => x.RoundId);
                    table.ForeignKey(
                        name: "FK_TournamentRounds_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentPairings",
                columns: table => new
                {
                    PairingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Duelist1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Duelist1Score = table.Column<int>(type: "int", nullable: false),
                    Duelist2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Duelist2Score = table.Column<int>(type: "int", nullable: false),
                    RoundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPairings", x => x.PairingId);
                    table.ForeignKey(
                        name: "FK_TournamentPairings_Duelists_Duelist1Id",
                        column: x => x.Duelist1Id,
                        principalTable: "Duelists",
                        principalColumn: "DuelistId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TournamentPairings_Duelists_Duelist2Id",
                        column: x => x.Duelist2Id,
                        principalTable: "Duelists",
                        principalColumn: "DuelistId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TournamentPairings_TournamentRounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "TournamentRounds",
                        principalColumn: "RoundId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPairings_Duelist1Id",
                table: "TournamentPairings",
                column: "Duelist1Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPairings_Duelist2Id",
                table: "TournamentPairings",
                column: "Duelist2Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPairings_RoundId_Duelist1Id_Duelist2Id",
                table: "TournamentPairings",
                columns: new[] { "RoundId", "Duelist1Id", "Duelist2Id" },
                unique: true,
                filter: "[Duelist2Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentRounds_TournamentId_RoundNumber",
                table: "TournamentRounds",
                columns: new[] { "TournamentId", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_BoosterPackInfoId",
                table: "Tournaments",
                column: "BoosterPackInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_ProgressionSeriesId_TournamentNumber",
                table: "Tournaments",
                columns: new[] { "ProgressionSeriesId", "TournamentNumber" },
                unique: true);
        }
    }
}
