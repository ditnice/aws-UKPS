using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptedPasswordToUserOnboardingRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "encrypted_password",
                schema: "ukps",
                table: "user_onboarding_records",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "password_expires_at",
                schema: "ukps",
                table: "user_onboarding_records",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "encrypted_password",
                schema: "ukps",
                table: "user_onboarding_records");

            migrationBuilder.DropColumn(
                name: "password_expires_at",
                schema: "ukps",
                table: "user_onboarding_records");
        }
    }
}
