using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentDuelist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tournaments_SeriesId",
                table: "Tournaments");

            migrationBuilder.CreateTable(
                name: "TournamentDuelist",
                columns: table => new
                {
                    TournamentDuelistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<int>(type: "int", nullable: false),
                    OpponentScore = table.Column<int>(type: "int", nullable: false),
                    OpponentOpponentScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentDuelist", x => x.TournamentDuelistId);
                    table.ForeignKey(
                        name: "FK_TournamentDuelist_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_SeriesId_Number",
                table: "Tournaments",
                columns: new[] { "SeriesId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TournamentDuelist_TournamentId_Username",
                table: "TournamentDuelist",
                columns: new[] { "TournamentId", "Username" },
                unique: true,
                filter: "[Username] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TournamentDuelist");

            migrationBuilder.DropIndex(
                name: "IX_Tournaments_SeriesId_Number",
                table: "Tournaments");

            migrationBuilder.CreateIndex(
                name: "IX_Tournaments_SeriesId",
                table: "Tournaments",
                column: "SeriesId");
        }
    }
}
