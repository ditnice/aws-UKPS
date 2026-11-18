using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalTrialDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "display_order",
                schema: "ukps",
                table: "record_clinical_trials",
                type: "integer",
                nullable: true);

            // Preserve the ID order previously used to display each revision's trials.
            migrationBuilder.Sql(
                """
                UPDATE ukps.record_clinical_trials AS trial
                SET display_order = ordered.position
                FROM (
                    SELECT id, ROW_NUMBER() OVER (
                        PARTITION BY revision_id ORDER BY id
                    )::integer AS position
                    FROM ukps.record_clinical_trials
                ) AS ordered
                WHERE trial.id = ordered.id;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_order",
                schema: "ukps",
                table: "record_clinical_trials");
        }
    }
}
