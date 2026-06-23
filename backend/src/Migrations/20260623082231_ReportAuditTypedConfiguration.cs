using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReportAuditTypedConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string[]>(
                name: "field_usage",
                schema: "ukps",
                table: "report_audits",
                type: "text[]",
                nullable: false,
                defaultValue: new string[0],
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "configuration",
                schema: "ukps",
                table: "report_audits",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "field_usage",
                schema: "ukps",
                table: "report_audits",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string[]),
                oldType: "text[]");

            migrationBuilder.AlterColumn<string>(
                name: "configuration",
                schema: "ukps",
                table: "report_audits",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb");
        }
    }
}
