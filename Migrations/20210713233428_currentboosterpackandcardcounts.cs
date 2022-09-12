using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class currentboosterpackandcardcounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrentBoosterPackId",
                table: "ProgressionSeries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "NumUniqueCards",
                table: "Duelists",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_ProgressionSeries_CurrentBoosterPackId",
                table: "ProgressionSeries",
                column: "CurrentBoosterPackId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressionSeries_BoosterPackInfos_CurrentBoosterPackId",
                table: "ProgressionSeries",
                column: "CurrentBoosterPackId",
                principalTable: "BoosterPackInfos",
                principalColumn: "BoosterPackInfoId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressionSeries_BoosterPackInfos_CurrentBoosterPackId",
                table: "ProgressionSeries");

            migrationBuilder.DropIndex(
                name: "IX_ProgressionSeries_CurrentBoosterPackId",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "CurrentBoosterPackId",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "NumUniqueCards",
                table: "Duelists");
        }
    }
}
