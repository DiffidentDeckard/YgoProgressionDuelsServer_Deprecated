using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentPairings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TournamentPairing",
                columns: table => new
                {
                    TournamentPairingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoundNumber = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentPairing", x => x.TournamentPairingId);
                    table.ForeignKey(
                        name: "FK_TournamentPairing_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PairingDuelist",
                columns: table => new
                {
                    PairingDuelistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PairingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PairingDuelist", x => x.PairingDuelistId);
                    table.ForeignKey(
                        name: "FK_PairingDuelist_TournamentPairing_PairingId",
                        column: x => x.PairingId,
                        principalTable: "TournamentPairing",
                        principalColumn: "TournamentPairingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PairingDuelist_PairingId_Username",
                table: "PairingDuelist",
                columns: new[] { "PairingId", "Username" },
                unique: true,
                filter: "[Username] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentPairing_TournamentId",
                table: "TournamentPairing",
                column: "TournamentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PairingDuelist");

            migrationBuilder.DropTable(
                name: "TournamentPairing");
        }
    }
}
