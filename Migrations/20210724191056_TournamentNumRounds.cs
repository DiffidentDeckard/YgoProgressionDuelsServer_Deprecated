using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class TournamentNumRounds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ExpectedNumRounds",
                table: "Tournaments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedNumRounds",
                table: "Tournaments");
        }
    }
}
