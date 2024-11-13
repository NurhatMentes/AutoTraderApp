using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderNullableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Signals_SignalId",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Orders",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AddColumn<Guid>(
                name: "BrokerAccountId1",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BrokerAccountId1",
                table: "Orders",
                column: "BrokerAccountId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId1",
                table: "Orders",
                column: "BrokerAccountId1",
                principalTable: "BrokerAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Signals_SignalId",
                table: "Orders",
                column: "SignalId",
                principalTable: "Signals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId1",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Signals_SignalId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BrokerAccountId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "BrokerAccountId1",
                table: "Orders");

            migrationBuilder.AlterColumn<string>(
                name: "RejectionReason",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Orders",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Signals_SignalId",
                table: "Orders",
                column: "SignalId",
                principalTable: "Signals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
