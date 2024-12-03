using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions");

            migrationBuilder.AlterColumn<Guid>(
                name: "BrokerAccountId",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<decimal>(
                name: "MarketValue",
                table: "Positions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions",
                column: "BrokerAccountId",
                principalTable: "BrokerAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "MarketValue",
                table: "Positions");

            migrationBuilder.AlterColumn<Guid>(
                name: "BrokerAccountId",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions",
                column: "BrokerAccountId",
                principalTable: "BrokerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
