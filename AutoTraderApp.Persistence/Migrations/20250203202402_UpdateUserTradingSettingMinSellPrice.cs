using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserTradingSettingMinBuyPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MinBuyPrice",
                table: "UserTradingSettings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinBuyPrice",
                table: "UserTradingSettings");
        }
    }
}
