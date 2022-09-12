using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace YgoProgressionDuels.Migrations
{
    public partial class BanListAndEntriesRework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanListEntry_ProgressionSeries_SeriesId",
                table: "BanListEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BanListEntry",
                table: "BanListEntry");

            migrationBuilder.DropIndex(
                name: "IX_BanListEntry_SeriesId_CardName",
                table: "BanListEntry");

            migrationBuilder.DropColumn(
                name: "CardName",
                table: "BanListEntry");

            migrationBuilder.RenameTable(
                name: "BanListEntry",
                newName: "BanListEntries");

            migrationBuilder.RenameColumn(
                name: "SeriesId",
                table: "BanListEntries",
                newName: "OwnerBanListId");

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentBanListId",
                table: "ProgressionSeries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CardInfoId",
                table: "BanListEntries",
                type: "decimal(20,0)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BanListEntries",
                table: "BanListEntries",
                column: "BanListEntryId");

            migrationBuilder.CreateTable(
                name: "BanLists",
                columns: table => new
                {
                    BanListId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanLists", x => x.BanListId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressionSeries_CurrentBanListId",
                table: "ProgressionSeries",
                column: "CurrentBanListId");

            migrationBuilder.CreateIndex(
                name: "IX_BanListEntries_CardInfoId",
                table: "BanListEntries",
                column: "CardInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_BanListEntries_OwnerBanListId_CardInfoId",
                table: "BanListEntries",
                columns: new[] { "OwnerBanListId", "CardInfoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BanLists_ReleaseDate",
                table: "BanLists",
                column: "ReleaseDate",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BanListEntries_BanLists_OwnerBanListId",
                table: "BanListEntries",
                column: "OwnerBanListId",
                principalTable: "BanLists",
                principalColumn: "BanListId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BanListEntries_CardInfos_CardInfoId",
                table: "BanListEntries",
                column: "CardInfoId",
                principalTable: "CardInfos",
                principalColumn: "CardInfoId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressionSeries_BanLists_CurrentBanListId",
                table: "ProgressionSeries",
                column: "CurrentBanListId",
                principalTable: "BanLists",
                principalColumn: "BanListId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BanListEntries_BanLists_OwnerBanListId",
                table: "BanListEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_BanListEntries_CardInfos_CardInfoId",
                table: "BanListEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressionSeries_BanLists_CurrentBanListId",
                table: "ProgressionSeries");

            migrationBuilder.DropTable(
                name: "BanLists");

            migrationBuilder.DropIndex(
                name: "IX_ProgressionSeries_CurrentBanListId",
                table: "ProgressionSeries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BanListEntries",
                table: "BanListEntries");

            migrationBuilder.DropIndex(
                name: "IX_BanListEntries_CardInfoId",
                table: "BanListEntries");

            migrationBuilder.DropIndex(
                name: "IX_BanListEntries_OwnerBanListId_CardInfoId",
                table: "BanListEntries");

            migrationBuilder.DropColumn(
                name: "CurrentBanListId",
                table: "ProgressionSeries");

            migrationBuilder.DropColumn(
                name: "CardInfoId",
                table: "BanListEntries");

            migrationBuilder.RenameTable(
                name: "BanListEntries",
                newName: "BanListEntry");

            migrationBuilder.RenameColumn(
                name: "OwnerBanListId",
                table: "BanListEntry",
                newName: "SeriesId");

            migrationBuilder.AddColumn<string>(
                name: "CardName",
                table: "BanListEntry",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BanListEntry",
                table: "BanListEntry",
                column: "BanListEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_BanListEntry_SeriesId_CardName",
                table: "BanListEntry",
                columns: new[] { "SeriesId", "CardName" },
                unique: true,
                filter: "[CardName] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_BanListEntry_ProgressionSeries_SeriesId",
                table: "BanListEntry",
                column: "SeriesId",
                principalTable: "ProgressionSeries",
                principalColumn: "ProgressionSeriesId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
