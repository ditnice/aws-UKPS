using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalisedEmailFunctionAndColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            CREATE FUNCTION public.normalise_email(text) RETURNS text
                LANGUAGE sql IMMUTABLE PARALLEL SAFE
                AS $$ SELECT UPPER($1 COLLATE "C") $$;
            """);

            migrationBuilder.DropIndex(
                name: "ix_app_user_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.AddColumn<string>(
                name: "normalised_work_email",
                schema: "ukps",
                table: "app_user",
                type: "text",
                nullable: false,
                computedColumnSql: "public.normalise_email(work_email)",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_app_user_normalised_work_email",
                schema: "ukps",
                table: "app_user",
                column: "normalised_work_email",
                unique: true);

            // Reject whitespace and control characters
            migrationBuilder.Sql("""
                ALTER TABLE ukps.app_user ADD CONSTRAINT work_email_no_whitespace
                CHECK (work_email !~ '[[:space:][:cntrl:]]');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE ukps.app_user DROP CONSTRAINT work_email_no_whitespace;");

            migrationBuilder.DropIndex(
                name: "ix_app_user_normalised_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.DropColumn(
                name: "normalised_work_email",
                schema: "ukps",
                table: "app_user");

            migrationBuilder.CreateIndex(
                name: "ix_app_user_work_email",
                schema: "ukps",
                table: "app_user",
                column: "work_email",
                unique: true);

            migrationBuilder.Sql("DROP FUNCTION public.normalise_email(text);");
        }
    }
}
