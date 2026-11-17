using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientPathwayPointCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "code",
                schema: "ukps",
                table: "patient_pathway_point",
                type: "text",
                nullable: true);

            // Existing rows get the code of their label, or a unique placeholder for a label the
            // seeder does not know, so the unique index can be created. The earlier labels ("At
            // diagnosis", "At disease progression or relapse", ...) were a different list rather
            // than renames, so only "Other", which both lists share, is mapped from them.
            migrationBuilder.Sql(
                """
                UPDATE ukps.patient_pathway_point
                SET code = CASE
                    WHEN label = 'Initial presentation' THEN 'initial-presentation'
                    WHEN label = 'Progression' THEN 'progression'
                    WHEN label = 'Longitudinal testing' THEN 'longitudinal-testing'
                    WHEN label = 'Other' THEN 'other'
                    ELSE 'unknown-' || id
                END;
                """
            );

            migrationBuilder.AlterColumn<string>(
                name: "code",
                schema: "ukps",
                table: "patient_pathway_point",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_patient_pathway_point_code",
                schema: "ukps",
                table: "patient_pathway_point",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_patient_pathway_point_code",
                schema: "ukps",
                table: "patient_pathway_point");

            migrationBuilder.DropColumn(
                name: "code",
                schema: "ukps",
                table: "patient_pathway_point");
        }
    }
}
