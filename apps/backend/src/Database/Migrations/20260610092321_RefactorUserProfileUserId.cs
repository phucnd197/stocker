using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stocker.Database.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUserProfileUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "UserProfiles",
                newName: "Auth0Sub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Auth0Sub",
                table: "UserProfiles",
                newName: "UserId");
        }
    }
}
