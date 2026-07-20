using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmailUniquenessConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_user_username",
                schema: "ukps",
                table: "app_user"
            );

            migrationBuilder.AlterColumn<string>(
                name: "work_email",
                schema: "ukps",
                table: "app_user",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_app_user_work_email",
                schema: "ukps",
                table: "app_user",
                column: "work_email",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_app_user_work_email",
                schema: "ukps",
                table: "app_user"
            );

            migrationBuilder.AlterColumn<string>(
                name: "work_email",
                schema: "ukps",
                table: "app_user",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text"
            );

            migrationBuilder.CreateIndex(
                name: "ix_app_user_username",
                schema: "ukps",
                table: "app_user",
                column: "username",
                unique: true
            );
        }
    }
}
