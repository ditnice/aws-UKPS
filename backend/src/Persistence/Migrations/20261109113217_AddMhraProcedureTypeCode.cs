using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMhraProcedureTypeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                schema: "ukps",
                table: "mhra_procedure_type",
                type: "text",
                nullable: true);

            // Existing rows get the code of their label, current or previous, or a unique
            // placeholder for a label the seeder does not know, so the unique index can be created.
            migrationBuilder.Sql(
                """
                UPDATE ukps.mhra_procedure_type
                SET code = CASE
                    WHEN label IN ('MHRA national procedure', 'MHRA National Procedure') THEN 'national'
                    WHEN label = 'International Recognition Procedure' THEN 'international-recognition'
                    WHEN label = 'Access Consortium' THEN 'access-consortium'
                    WHEN label IN ('Orbis', 'Project Orbis') THEN 'orbis'
                    WHEN label = 'Other' THEN 'other'
                    WHEN label IN ('Unknown at this stage', 'Not yet confirmed') THEN 'unknown-at-this-stage'
                    ELSE 'unknown-' || id
                END;
                """
            );

            migrationBuilder.AlterColumn<string>(
                name: "code",
                schema: "ukps",
                table: "mhra_procedure_type",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_mhra_procedure_type_code",
                schema: "ukps",
                table: "mhra_procedure_type",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_mhra_procedure_type_code",
                schema: "ukps",
                table: "mhra_procedure_type");

            migrationBuilder.DropColumn(
                name: "code",
                schema: "ukps",
                table: "mhra_procedure_type");
        }
    }
}
