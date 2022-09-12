using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentsAndScores : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Tournament",
                columns: table => new
                {
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProgressionSeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoosterPackInfoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TournamentNumber = table.Column<long>(type: "bigint", nullable: false),
                    CurrentRoundNumber = table.Column<long>(type: "bigint", nullable: false),
                    DateStarted = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournament", x => x.TournamentId);
                    table.ForeignKey(
                        name: "FK_Tournament_BoosterPackInfos_BoosterPackInfoId",
                        column: x => x.BoosterPackInfoId,
                        principalTable: "BoosterPackInfos",
                        principalColumn: "BoosterPackInfoId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tournament_ProgressionSeries_ProgressionSeriesId",
                        column: x => x.ProgressionSeriesId,
                        principalTable: "ProgressionSeries",
                        principalColumn: "ProgressionSeriesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentPairing",
                columns: table => new
                {
                    PairingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundNumber = table.Column<long>(type: "bigint", nullable: false),
                    Duelist1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Duelist1Score = table.Column<int>(type: "int", nullable: false),
                    Duelist2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Duelist2Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPairing", x => x.PairingId);
                    table.ForeignKey(
                        name: "FK_TournamentPairing_Duelists_Duelist1Id",
                        column: x => x.Duelist1Id,
                        principalTable: "Duelists",
                        principalColumn: "DuelistId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TournamentPairing_Duelists_Duelist2Id",
                        column: x => x.Duelist2Id,
                        principalTable: "Duelists",
                        principalColumn: "DuelistId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TournamentPairing_Tournament_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournament",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tournament_BoosterPackInfoId",
                table: "Tournament",
                column: "BoosterPackInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Tournament_ProgressionSeriesId_TournamentNumber",
                table: "Tournament",
                columns: new[] { "ProgressionSeriesId", "TournamentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPairing_Duelist1Id",
                table: "TournamentPairing",
                column: "Duelist1Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPairing_Duelist2Id",
                table: "TournamentPairing",
                column: "Duelist2Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPairing_TournamentId_Duelist1Id_Duelist2Id",
                table: "TournamentPairing",
                columns: new[] { "TournamentId", "Duelist1Id", "Duelist2Id" },
                unique: true,
                filter: "[Duelist2Id] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentPairing");

            migrationBuilder.DropTable(
                name: "Tournament");

            migrationBuilder.DropColumn(
                name: "CurrentTournamentScore",
                table: "Duelists");

            migrationBuilder.DropColumn(
                name: "TotalTournamentsScore",
                table: "Duelists");
        }
    }
}
