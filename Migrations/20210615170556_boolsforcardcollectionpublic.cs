using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class boolsforcardcollectionpublic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CardCollectionIsPublic",
                table: "Duelists",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeckIsPublic",
                table: "Duelists",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardCollectionIsPublic",
                table: "Duelists");

            migrationBuilder.DropColumn(
                name: "DeckIsPublic",
                table: "Duelists");
        }
    }
}
