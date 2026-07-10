using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateToMakeUsernameAMandatoryUniqueColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_app_user_username",
                schema: "ukps",
                table: "app_user",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_user_username",
                schema: "ukps",
                table: "app_user");
        }
    }
}
