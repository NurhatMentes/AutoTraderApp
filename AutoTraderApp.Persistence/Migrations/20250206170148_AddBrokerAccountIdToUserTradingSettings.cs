using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTraderApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerAccountIdToUserTradingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
     name: "BrokerAccountId",
     table: "UserTradingSettings",
     type: "uniqueidentifier",
     nullable: true); 
            migrationBuilder.CreateIndex(
    name: "IX_UserTradingSettings_BrokerAccountId",
    table: "UserTradingSettings",
    column: "BrokerAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTradingSettings_BrokerAccounts_BrokerAccountId",
                table: "UserTradingSettings",
                column: "BrokerAccountId",
                principalTable: "BrokerAccounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull); 

        }
    }
}
