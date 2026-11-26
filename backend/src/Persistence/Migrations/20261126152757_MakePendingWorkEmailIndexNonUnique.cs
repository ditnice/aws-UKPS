using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakePendingWorkEmailIndexNonUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_user_normalised_pending_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_normalised_pending_work_email",
                schema: "ukps",
                table: "app_user",
                column: "normalised_pending_work_email",
                filter: "normalised_pending_work_email IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_user_normalised_pending_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_normalised_pending_work_email",
                schema: "ukps",
                table: "app_user",
                column: "normalised_pending_work_email",
                unique: true,
                filter: "normalised_pending_work_email IS NOT NULL");
        }
    }
}
