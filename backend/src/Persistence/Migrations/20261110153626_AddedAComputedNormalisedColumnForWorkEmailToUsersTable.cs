using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedAComputedNormalisedColumnForWorkEmailToUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "normalised_work_email",
                schema: "ukps",
                table: "app_user",
                type: "text",
                nullable: false,
                computedColumnSql: "UPPER(TRIM(work_email))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_user_normalised_work_email",
                schema: "ukps",
                table: "app_user",
                column: "normalised_work_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_user_normalised_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "normalised_work_email",
                schema: "ukps",
                table: "app_user");
        }
    }
}
