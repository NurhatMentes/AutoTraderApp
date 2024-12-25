using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdxSmoothing",
                table: "Strategies",
                type: "int",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddColumn<float>(
                name: "AdxThreshold",
                table: "Strategies",
                type: "real",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<int>(
                name: "AtrLength",
                table: "Strategies",
                type: "int",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddColumn<int>(
                name: "BollingerLength",
                table: "Strategies",
                type: "int",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<float>(
                name: "BollingerMultiplier",
                table: "Strategies",
                type: "real",
                nullable: false,
                defaultValue: 1.5f);

            migrationBuilder.AddColumn<int>(
                name: "DmiLength",
                table: "Strategies",
                type: "int",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddColumn<int>(
                name: "RsiLength",
                table: "Strategies",
                type: "int",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddColumn<float>(
                name: "RsiLower",
                table: "Strategies",
                type: "real",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<float>(
                name: "RsiUpper",
                table: "Strategies",
                type: "real",
                nullable: false,
                defaultValue: 70);

            migrationBuilder.AddColumn<int>(
                name: "StochRsiLength",
                table: "Strategies",
                type: "int",
                nullable: false,
                defaultValue: 14);

            migrationBuilder.AddColumn<float>(
                name: "StochRsiLower",
                table: "Strategies",
                type: "real",
                nullable: false,
                defaultValue: 0.2f);

            migrationBuilder.AddColumn<float>(
                name: "StochRsiUpper",
                table: "Strategies",
                type: "real",
                nullable: false,
                defaultValue: 0.8f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdxSmoothing",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "AdxThreshold",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "AtrLength",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "BollingerLength",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "BollingerMultiplier",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "DmiLength",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "RsiLength",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "RsiLower",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "RsiUpper",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "StochRsiLength",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "StochRsiLower",
                table: "Strategies");

            migrationBuilder.DropColumn(
                name: "StochRsiUpper",
                table: "Strategies");
        }
    }
}
