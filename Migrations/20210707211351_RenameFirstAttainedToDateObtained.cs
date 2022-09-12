using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class RenameFirstAttainedToDateObtained : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateFirstAttained",
                table: "Cards",
                newName: "DateObtained");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateObtained",
                table: "Cards",
                newName: "DateFirstAttained");
        }
    }
}
