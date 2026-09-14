using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddingApprovalMetaDataToTheUserMembershipRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "approved_at",
                schema: "ukps",
                table: "user_registration_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_registration_requests_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests",
                column: "approved_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_registration_requests_users_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests",
                column: "approved_by_user_id",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_registration_requests_users_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests");

            migrationBuilder.DropIndex(
                name: "ix_user_registration_requests_approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "approved_at",
                schema: "ukps",
                table: "user_registration_requests");

            migrationBuilder.DropColumn(
                name: "approved_by_user_id",
                schema: "ukps",
                table: "user_registration_requests");
        }
    }
}
