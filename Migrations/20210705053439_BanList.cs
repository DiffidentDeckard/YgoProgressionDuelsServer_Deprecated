using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class BanList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanListEntry",
                columns: table => new
                {
                    BanListEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SeriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CardName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BanListStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanListEntry", x => x.BanListEntryId);
                    table.ForeignKey(
                        name: "FK_BanListEntry_ProgressionSeries_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "ProgressionSeries",
                        principalColumn: "ProgressionSeriesId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BanListEntry_SeriesId_CardName",
                table: "BanListEntry",
                columns: new[] { "SeriesId", "CardName" },
                unique: true,
                filter: "[CardName] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanListEntry");
        }
    }
}
