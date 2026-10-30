using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPasswordResetRequestsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_password_reset_requests",
                schema: "ukps",
                columns: table => new
                {
                    token_hash = table.Column<byte[]>(type: "bytea", nullable: false),
                    target_email_address = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_reset_requests", x => x.token_hash);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_password_reset_requests_target_email_address_created_at",
                schema: "ukps",
                table: "user_password_reset_requests",
                columns: new[] { "target_email_address", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_password_reset_requests",
                schema: "ukps");
        }
    }
}
