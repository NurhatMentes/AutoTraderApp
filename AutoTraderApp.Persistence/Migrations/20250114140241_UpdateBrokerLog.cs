using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBrokerLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
            name: "BrokerLogs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BrokerAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Price = table.Column<decimal>(type: "nvarchar(max)", nullable: true),
                Quantity = table.Column<int>(type: "nvarchar(max)", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BrokerLogs", x => x.Id);
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
             name: "BrokerLogs");
        }
    }
}
