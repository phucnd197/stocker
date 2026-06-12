using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stocker.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class StockSymbol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PortfolioStocks_Company",
                table: "PortfolioStocks",
                column: "Company",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PortfolioStocks_Company",
                table: "PortfolioStocks");
        }
    }
}
