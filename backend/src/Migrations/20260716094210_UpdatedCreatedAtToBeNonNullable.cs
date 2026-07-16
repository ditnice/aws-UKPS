using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedCreatedAtToBeNonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "ukps",
                table: "terms_acceptances",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "ukps",
                table: "organisations",
                type: "timestamptz",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamptz",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "ukps",
                table: "terms_acceptances",
                type: "timestamptz",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                schema: "ukps",
                table: "organisations",
                type: "timestamptz",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamptz");
        }
    }
}
