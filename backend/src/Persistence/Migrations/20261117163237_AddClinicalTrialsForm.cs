using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicalTrialsForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_record_clinical_trial_information_revision_id",
                schema: "ukps",
                table: "record_clinical_trial_information");

            migrationBuilder.AddColumn<int>(
                name: "has_clinical_trials",
                schema: "ukps",
                table: "record_clinical_trial_information",
                type: "integer",
                nullable: true);

            // Trials now hang off their revision's information row, so every revision with trials
            // needs one, and has answered yes (YesNo.Yes = 1) to adding them.
            migrationBuilder.Sql(
                """
                INSERT INTO ukps.record_clinical_trial_information (revision_id)
                SELECT DISTINCT t.revision_id
                FROM ukps.record_clinical_trials t
                WHERE NOT EXISTS (
                    SELECT 1 FROM ukps.record_clinical_trial_information i
                    WHERE i.revision_id = t.revision_id
                );

                UPDATE ukps.record_clinical_trial_information i
                SET has_clinical_trials = 1
                WHERE EXISTS (
                    SELECT 1 FROM ukps.record_clinical_trials t
                    WHERE t.revision_id = i.revision_id
                );
                """
            );

            // The alternate key's unique constraint replaces the dropped unique index, so a
            // revision still has at most one information row.
            migrationBuilder.AddUniqueConstraint(
                name: "ak_record_clinical_trial_information_revision_id",
                schema: "ukps",
                table: "record_clinical_trial_information",
                column: "revision_id");

            migrationBuilder.AddForeignKey(
                name: "fk_record_clinical_trials_record_clinical_trial_information_re",
                schema: "ukps",
                table: "record_clinical_trials",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_clinical_trial_information",
                principalColumn: "revision_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The information rows Up() added for revisions with trials are kept: they are valid
            // rows with no answers once has_clinical_trials is dropped.
            migrationBuilder.DropForeignKey(
                name: "fk_record_clinical_trials_record_clinical_trial_information_re",
                schema: "ukps",
                table: "record_clinical_trials");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_record_clinical_trial_information_revision_id",
                schema: "ukps",
                table: "record_clinical_trial_information");

            migrationBuilder.DropColumn(
                name: "has_clinical_trials",
                schema: "ukps",
                table: "record_clinical_trial_information");

            migrationBuilder.CreateIndex(
                name: "ix_record_clinical_trial_information_revision_id",
                schema: "ukps",
                table: "record_clinical_trial_information",
                column: "revision_id",
                unique: true);
        }
    }
}
