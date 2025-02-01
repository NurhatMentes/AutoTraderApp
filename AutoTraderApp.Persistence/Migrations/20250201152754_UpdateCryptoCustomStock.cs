using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCryptoCustomStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Crypto",
                table: "CryptoCustomStocks",
                newName: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "CryptoCustomStocks",
                newName: "Crypto");
        }
    }
}
