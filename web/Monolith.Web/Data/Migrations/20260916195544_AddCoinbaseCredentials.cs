using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Monolith.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCoinbaseCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoinbaseCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiKeyName = table.Column<string>(type: "TEXT", nullable: false),
                    ProtectedPrivateKey = table.Column<string>(type: "TEXT", nullable: false),
                    LiveTradingEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    WithdrawalsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoinbaseCredentials", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoinbaseCredentials");
        }
    }
}
