using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPositionProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions");

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenedAt",
                table: "Positions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PositionId",
                table: "Orders",
                column: "PositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Positions_PositionId",
                table: "Orders",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Positions_PositionId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PositionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OpenedAt",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
