using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTradeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId1",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Instruments_InstrumentId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Positions_PositionId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Signals_SignalId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Strategies_StrategyId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Users_UserId",
                table: "Positions");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Orders_OrderId",
                table: "Trades");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Positions_PositionId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_OrderId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Trades_PositionId",
                table: "Trades");

            migrationBuilder.DropIndex(
                name: "IX_Positions_InstrumentId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_StrategyId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Positions_UserId",
                table: "Positions");

            migrationBuilder.DropIndex(
                name: "IX_Orders_BrokerAccountId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_InstrumentId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_PositionId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SignalId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Commission",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ExecutedPrice",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ExecutedQuantity",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ExternalTradeId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "ClosedAt",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "InstrumentId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "OpenedAt",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Side",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "StopLoss",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "StrategyId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "TakeProfit",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "BrokerAccountId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ExternalOrderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InstrumentId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PositionId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SignalId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "ExecutedAt",
                table: "Trades",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "TakeProfit",
                table: "Orders",
                newName: "StopPrice");

            migrationBuilder.RenameColumn(
                name: "StopLoss",
                table: "Orders",
                newName: "LimitPrice");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Orders",
                newName: "FilledQuantity");

            migrationBuilder.RenameColumn(
                name: "IsPaperTrading",
                table: "BrokerAccounts",
                newName: "IsPaper");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Trades",
                type: "decimal(18,8)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Trades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Trades",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Positions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Side",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,8)",
                oldPrecision: 18,
                oldScale: 8);

            migrationBuilder.AddColumn<decimal>(
                name: "FilledPrice",
                table: "Orders",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symbol",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeInForce",
                table: "Orders",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId",
                table: "Orders",
                column: "BrokerAccountId",
                principalTable: "BrokerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions",
                column: "BrokerAccountId",
                principalTable: "BrokerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Trades");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "FilledPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Symbol",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TimeInForce",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Trades",
                newName: "ExecutedAt");

            migrationBuilder.RenameColumn(
                name: "StopPrice",
                table: "Orders",
                newName: "TakeProfit");

            migrationBuilder.RenameColumn(
                name: "LimitPrice",
                table: "Orders",
                newName: "StopLoss");

            migrationBuilder.RenameColumn(
                name: "FilledQuantity",
                table: "Orders",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "IsPaper",
                table: "BrokerAccounts",
                newName: "IsPaperTrading");

            migrationBuilder.AddColumn<decimal>(
                name: "Commission",
                table: "Trades",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExecutedPrice",
                table: "Trades",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExecutedQuantity",
                table: "Trades",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExternalTradeId",
                table: "Trades",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Trades",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "Trades",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "Positions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstrumentId",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "OpenedAt",
                table: "Positions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Side",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "StopLoss",
                table: "Positions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StrategyId",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TakeProfit",
                table: "Positions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Side",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "Orders",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "BrokerAccountId1",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalOrderId",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstrumentId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PositionId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SignalId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OrderId",
                table: "Trades",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_PositionId",
                table: "Trades",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_InstrumentId",
                table: "Positions",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_StrategyId",
                table: "Positions",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_UserId",
                table: "Positions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_BrokerAccountId1",
                table: "Orders",
                column: "BrokerAccountId1");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_InstrumentId",
                table: "Orders",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_PositionId",
                table: "Orders",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SignalId",
                table: "Orders",
                column: "SignalId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId",
                table: "Orders",
                column: "BrokerAccountId",
                principalTable: "BrokerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_BrokerAccounts_BrokerAccountId1",
                table: "Orders",
                column: "BrokerAccountId1",
                principalTable: "BrokerAccounts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Instruments_InstrumentId",
                table: "Orders",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Positions_PositionId",
                table: "Orders",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Signals_SignalId",
                table: "Orders",
                column: "SignalId",
                principalTable: "Signals",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_BrokerAccounts_BrokerAccountId",
                table: "Positions",
                column: "BrokerAccountId",
                principalTable: "BrokerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Instruments_InstrumentId",
                table: "Positions",
                column: "InstrumentId",
                principalTable: "Instruments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Strategies_StrategyId",
                table: "Positions",
                column: "StrategyId",
                principalTable: "Strategies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Users_UserId",
                table: "Positions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Orders_OrderId",
                table: "Trades",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Positions_PositionId",
                table: "Trades",
                column: "PositionId",
                principalTable: "Positions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
