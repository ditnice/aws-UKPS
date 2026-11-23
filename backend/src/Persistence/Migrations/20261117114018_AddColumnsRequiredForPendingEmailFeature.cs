using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsRequiredForPendingEmailFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "pending_email_verified_at",
                schema: "ukps",
                table: "app_user",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pending_work_email",
                schema: "ukps",
                table: "app_user",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "normalised_pending_work_email",
                schema: "ukps",
                table: "app_user",
                type: "text",
                nullable: true,
                computedColumnSql: "ukps.normalise_email(pending_work_email)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_user_normalised_pending_work_email",
                schema: "ukps",
                table: "app_user",
                column: "normalised_pending_work_email",
                unique: true,
                filter: "normalised_pending_work_email IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_user_normalised_pending_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "normalised_pending_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "pending_email_verified_at",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "pending_work_email",
                schema: "ukps",
                table: "app_user");
        }
    }
}
