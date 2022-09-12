using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentDuelist2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentDuelist_Tournaments_TournamentId",
                table: "TournamentDuelist");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentDuelist",
                table: "TournamentDuelist");

            migrationBuilder.RenameTable(
                name: "TournamentDuelist",
                newName: "TournamentDuelists");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentDuelist_TournamentId_Username",
                table: "TournamentDuelists",
                newName: "IX_TournamentDuelists_TournamentId_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentDuelists",
                table: "TournamentDuelists",
                column: "TournamentDuelistId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentDuelists_Tournaments_TournamentId",
                table: "TournamentDuelists",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TournamentDuelists_Tournaments_TournamentId",
                table: "TournamentDuelists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentDuelists",
                table: "TournamentDuelists");

            migrationBuilder.RenameTable(
                name: "TournamentDuelists",
                newName: "TournamentDuelist");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentDuelists_TournamentId_Username",
                table: "TournamentDuelist",
                newName: "IX_TournamentDuelist_TournamentId_Username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentDuelist",
                table: "TournamentDuelist",
                column: "TournamentDuelistId");

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentDuelist_Tournaments_TournamentId",
                table: "TournamentDuelist",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
