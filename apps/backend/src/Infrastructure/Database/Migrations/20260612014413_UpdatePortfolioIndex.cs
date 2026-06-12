using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stocker.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePortfolioIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PortfolioStocks_Company",
                table: "PortfolioStocks");

            migrationBuilder.DropIndex(
                name: "IX_PortfolioStocks_PortfolioId",
                table: "PortfolioStocks");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioStocks_PortfolioId_Company",
                table: "PortfolioStocks",
                columns: new[] { "PortfolioId", "Company" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PortfolioStocks_PortfolioId_Company",
                table: "PortfolioStocks");

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioStocks_Company",
                table: "PortfolioStocks",
                column: "Company",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortfolioStocks_PortfolioId",
                table: "PortfolioStocks",
                column: "PortfolioId");
        }
    }
}
