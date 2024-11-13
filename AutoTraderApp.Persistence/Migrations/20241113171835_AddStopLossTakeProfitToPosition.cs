using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStopLossTakeProfitToPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "StopLoss",
                table: "Positions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TakeProfit",
                table: "Positions",
                type: "decimal(18,8)",
                precision: 18,
                scale: 8,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StopLoss",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "TakeProfit",
                table: "Positions");
        }
    }
}
