using Microsoft.EntityFrameworkCore.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTradingViewLogConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradingViewLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    StrategyId = table.Column<Guid>(nullable: false),
                    BrokerAccountId = table.Column<Guid>(nullable: false),
                    Step = table.Column<string>(maxLength: 150, nullable: false),
                    Status = table.Column<string>(maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(maxLength: 20, nullable: false),
                    Message = table.Column<string>(maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingViewLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradingViewLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradingViewLogs_BrokerAccounts_BrokerAccountId",
                        column: x => x.BrokerAccountId,
                        principalTable: "BrokerAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradingViewLogs_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradingViewLogs_UserId",
                table: "TradingViewLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingViewLogs_StrategyId",
                table: "TradingViewLogs",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingViewLogs_BrokerAccountId",
                table: "TradingViewLogs",
                column: "BrokerAccountId");
        }

    }
}
