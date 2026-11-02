using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordContentForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                schema: "ukps",
                table: "therapeutic_areas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "code",
                schema: "ukps",
                table: "therapeutic_areas");
        }
    }
}
