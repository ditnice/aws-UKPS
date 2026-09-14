using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserApprovalMetaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_registration_requests_users_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests");

            migrationBuilder.AddForeignKey(
                name: "fk_user_registration_requests_users_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests",
                column: "approved_by_user_id",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_registration_requests_users_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests");

            migrationBuilder.AddForeignKey(
                name: "fk_user_registration_requests_users_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests",
                column: "approved_by_user_id",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id");
        }
    }
}
