using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithFinalDataset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Drop the genomic_sample_type lookup and its FK ───────────────
            migrationBuilder.DropForeignKey(
                name: "fk_medicines_laboratory_testings_genomic_sample_types_genomic_",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropIndex(
                name: "ix_medicines_laboratory_testings_genomic_sample_type_id",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropTable(
                name: "genomic_sample_type",
                schema: "ukps");

            // ── Drop the redundant vaccines_populations table ────────────────
            migrationBuilder.DropTable(
                name: "vaccines_populations",
                schema: "ukps");

            // ── Rename record_global_submissions → medicines_global_submissions
            migrationBuilder.RenameTable(
                name: "record_global_submissions",
                schema: "ukps",
                newName: "medicines_global_submissions",
                newSchema: "ukps");

            migrationBuilder.RenameIndex(
                name: "ix_record_global_submission_revision_id",
                schema: "ukps",
                table: "medicines_global_submissions",
                newName: "ix_medicines_global_submission_revision_id");

            migrationBuilder.RenameIndex(
                name: "ix_record_global_submissions_global_submission_actual_date_id",
                schema: "ukps",
                table: "medicines_global_submissions",
                newName: "ix_medicines_global_submissions_global_submission_actual_date_");

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_global_submissions
                    RENAME CONSTRAINT pk_record_global_submissions
                    TO pk_medicines_global_submissions;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_global_submissions
                    RENAME CONSTRAINT fk_record_global_submissions_record_revisions_revision_id
                    TO fk_medicines_global_submissions_record_revisions_revision_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_global_submissions
                    RENAME CONSTRAINT fk_record_global_submissions_regulatory_dates_global_submissio
                    TO fk_medicines_global_submissions_regulatory_dates_global_submis;
                """);

            // Drop the estimated-date FK, its index and the removed columns.
            migrationBuilder.DropForeignKey(
                name: "fk_record_global_submissions_regulatory_dates_global_submissio1",
                schema: "ukps",
                table: "medicines_global_submissions");

            migrationBuilder.DropIndex(
                name: "ix_record_global_submissions_global_submission_estimated_date_",
                schema: "ukps",
                table: "medicines_global_submissions");

            migrationBuilder.DropColumn(
                name: "global_first_submission_notes",
                schema: "ukps",
                table: "medicines_global_submissions");

            migrationBuilder.DropColumn(
                name: "global_submission_estimated_date_id",
                schema: "ukps",
                table: "medicines_global_submissions");

            // ── Rename record_intl_recognitions → medicines_intl_recognitions
            migrationBuilder.RenameTable(
                name: "record_intl_recognitions",
                schema: "ukps",
                newName: "medicines_intl_recognitions",
                newSchema: "ukps");

            migrationBuilder.RenameIndex(
                name: "ix_record_intl_recognition_revision_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_medicines_intl_recognition_revision_id");

            migrationBuilder.RenameIndex(
                name: "ix_record_intl_recognitions_intl_licence_date_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_medicines_intl_recognitions_intl_licence_date_id");

            migrationBuilder.RenameIndex(
                name: "ix_record_intl_recognitions_intl_submission_date_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_medicines_intl_recognitions_intl_submission_date_id");

            migrationBuilder.RenameIndex(
                name: "ix_record_intl_recognitions_irp_reference_regulator_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_medicines_intl_recognitions_irp_reference_regulator_id");

            migrationBuilder.RenameIndex(
                name: "ix_record_intl_recognitions_irp_route_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_medicines_intl_recognitions_irp_route_id");

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT pk_record_intl_recognitions
                    TO pk_medicines_intl_recognitions;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_record_intl_recognitions_irp_reference_regulators_irp_refer
                    TO fk_medicines_intl_recognitions_irp_reference_regulators_irp_re;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_record_intl_recognitions_irp_routes_irp_route_id
                    TO fk_medicines_intl_recognitions_irp_routes_irp_route_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_record_intl_recognitions_record_revisions_revision_id
                    TO fk_medicines_intl_recognitions_record_revisions_revision_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_record_intl_recognitions_regulatory_dates_intl_licence_date
                    TO fk_medicines_intl_recognitions_regulatory_dates_intl_licence_d;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_record_intl_recognitions_regulatory_dates_intl_submission_d
                    TO fk_medicines_intl_recognitions_regulatory_dates_intl_submissio;
                """);

            // ── medicines_product_details ────────────────────────────────────
            migrationBuilder.DropForeignKey(
                name: "fk_medicines_product_details_therapeutic_areas_therapeutic_are",
                schema: "ukps",
                table: "medicines_product_details");

            migrationBuilder.DropIndex(
                name: "ix_medicines_product_details_therapeutic_area_id",
                schema: "ukps",
                table: "medicines_product_details");

            migrationBuilder.DropColumn(
                name: "therapeutic_area_id",
                schema: "ukps",
                table: "medicines_product_details");

            migrationBuilder.AddColumn<int>(
                name: "indication_is_rare_disease",
                schema: "ukps",
                table: "medicines_product_details",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nice_ta_development_id",
                schema: "ukps",
                table: "medicines_product_details",
                type: "text",
                nullable: true);

            // ── medicines_service_impacts ────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "existing_nhs_service",
                schema: "ukps",
                table: "medicines_service_impacts");

            migrationBuilder.DropColumn(
                name: "nhs_service_redesign_details",
                schema: "ukps",
                table: "medicines_service_impacts");

            migrationBuilder.AddColumn<int>(
                name: "nhs_service_changes_required",
                schema: "ukps",
                table: "medicines_service_impacts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nhs_service_changes_details",
                schema: "ukps",
                table: "medicines_service_impacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "handling_storage_requirements",
                schema: "ukps",
                table: "medicines_service_impacts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "handling_storage_details",
                schema: "ukps",
                table: "medicines_service_impacts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "estimated_uptake",
                schema: "ukps",
                table: "medicines_service_impacts",
                type: "text",
                nullable: true);

            // ── medicines_budget_impacts ─────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "net_uk_budget_impact_over5m",
                schema: "ukps",
                table: "medicines_budget_impacts");

            migrationBuilder.AddColumn<int>(
                name: "net_uk_budget_impact_band",
                schema: "ukps",
                table: "medicines_budget_impacts",
                type: "integer",
                nullable: true);

            // ── medicines_laboratory_testings ────────────────────────────────
            migrationBuilder.DropColumn(
                name: "genomic_sample_type_id",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_sample_type_other",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_test_required",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_test_in_national_directory",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "national_genomic_test_directory_id",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_turnaround_considerations",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_biomarker",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_co_mutations",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_test_mandatory",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "monitoring_tests_required",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.AddColumn<int>(
                name: "biomarker_type",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "non_genomic_biomarker_description",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "genomic_target",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "genomic_test_ngtd_relationship",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "genomic_sample_type",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "genomic_turnaround_time_details",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "genomic_test_mandatory_status",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "additional_genomic_factors",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "safety_tests_details",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            // ── record_clinical_trials ───────────────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "trial_phase",
                schema: "ukps",
                table: "record_clinical_trials",
                type: "integer",
                nullable: true);

            // ── vaccines_company_infos ───────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "originator_company_name",
                schema: "ukps",
                table: "vaccines_company_infos");

            migrationBuilder.DropColumn(
                name: "has_been_acquired",
                schema: "ukps",
                table: "vaccines_company_infos");

            migrationBuilder.DropColumn(
                name: "previous_owner",
                schema: "ukps",
                table: "vaccines_company_infos");

            migrationBuilder.AddColumn<string>(
                name: "originator_details",
                schema: "ukps",
                table: "vaccines_company_infos",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "has_grant_funding",
                schema: "ukps",
                table: "vaccines_company_infos",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            // ── vaccines_service_readinesses ─────────────────────────────────
            migrationBuilder.DropColumn(
                name: "requires_reconstitution",
                schema: "ukps",
                table: "vaccines_service_readinesses");

            migrationBuilder.AddColumn<string>(
                name: "storage_requirement_other",
                schema: "ukps",
                table: "vaccines_service_readinesses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "dosing_schedule",
                schema: "ukps",
                table: "vaccines_service_readinesses",
                type: "text",
                nullable: false,
                defaultValue: "");

            // ── New tables ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "medicines_product_detail_therapeutic_areas",
                schema: "ukps",
                columns: table => new
                {
                    medicines_product_detail_id = table.Column<int>(type: "integer", nullable: false),
                    therapeutic_area_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_product_detail_therapeutic_areas", x => new { x.medicines_product_detail_id, x.therapeutic_area_id });
                    table.ForeignKey(
                        name: "fk_medicines_product_detail_therapeutic_areas_medicines_produc",
                        column: x => x.medicines_product_detail_id,
                        principalSchema: "ukps",
                        principalTable: "medicines_product_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_medicines_product_detail_therapeutic_areas_therapeutic_area",
                        column: x => x.therapeutic_area_id,
                        principalSchema: "ukps",
                        principalTable: "therapeutic_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_intl_submissions",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    has_intl_submission = table.Column<int>(type: "integer", nullable: true),
                    intl_submission_details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_intl_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_intl_submissions_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_detail_therapeutic_areas_therapeutic_area",
                schema: "ukps",
                table: "medicines_product_detail_therapeutic_areas",
                column: "therapeutic_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_intl_submission_revision_id",
                schema: "ukps",
                table: "vaccines_intl_submissions",
                column: "revision_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medicines_product_detail_therapeutic_areas",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_intl_submissions",
                schema: "ukps");

            // ── vaccines_service_readinesses ─────────────────────────────────
            migrationBuilder.DropColumn(
                name: "dosing_schedule",
                schema: "ukps",
                table: "vaccines_service_readinesses");

            migrationBuilder.DropColumn(
                name: "storage_requirement_other",
                schema: "ukps",
                table: "vaccines_service_readinesses");

            migrationBuilder.AddColumn<int>(
                name: "requires_reconstitution",
                schema: "ukps",
                table: "vaccines_service_readinesses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // ── vaccines_company_infos ───────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "originator_details",
                schema: "ukps",
                table: "vaccines_company_infos");

            migrationBuilder.AlterColumn<int>(
                name: "has_grant_funding",
                schema: "ukps",
                table: "vaccines_company_infos",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "originator_company_name",
                schema: "ukps",
                table: "vaccines_company_infos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "has_been_acquired",
                schema: "ukps",
                table: "vaccines_company_infos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "previous_owner",
                schema: "ukps",
                table: "vaccines_company_infos",
                type: "text",
                nullable: true);

            // ── record_clinical_trials ───────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "trial_phase",
                schema: "ukps",
                table: "record_clinical_trials");

            // ── medicines_laboratory_testings ────────────────────────────────
            migrationBuilder.DropColumn(
                name: "additional_genomic_factors",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "biomarker_type",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_sample_type",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_target",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_test_mandatory_status",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_test_ngtd_relationship",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "genomic_turnaround_time_details",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "non_genomic_biomarker_description",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.DropColumn(
                name: "safety_tests_details",
                schema: "ukps",
                table: "medicines_laboratory_testings");

            migrationBuilder.AddColumn<int>(
                name: "genomic_sample_type_id",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "genomic_sample_type_other",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "genomic_test_required",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "genomic_test_in_national_directory",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "national_genomic_test_directory_id",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "genomic_turnaround_considerations",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "genomic_biomarker",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "genomic_co_mutations",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "genomic_test_mandatory",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "monitoring_tests_required",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                type: "integer",
                nullable: true);

            // ── medicines_budget_impacts ─────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "net_uk_budget_impact_band",
                schema: "ukps",
                table: "medicines_budget_impacts");

            migrationBuilder.AddColumn<int>(
                name: "net_uk_budget_impact_over5m",
                schema: "ukps",
                table: "medicines_budget_impacts",
                type: "integer",
                nullable: true);

            // ── medicines_service_impacts ────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "estimated_uptake",
                schema: "ukps",
                table: "medicines_service_impacts");

            migrationBuilder.DropColumn(
                name: "handling_storage_details",
                schema: "ukps",
                table: "medicines_service_impacts");

            migrationBuilder.DropColumn(
                name: "handling_storage_requirements",
                schema: "ukps",
                table: "medicines_service_impacts");

            migrationBuilder.DropColumn(
                name: "nhs_service_changes_details",
                schema: "ukps",
                table: "medicines_service_impacts");

            migrationBuilder.DropColumn(
                name: "nhs_service_changes_required",
                schema: "ukps",
                table: "medicines_service_impacts");

            migrationBuilder.AddColumn<int>(
                name: "existing_nhs_service",
                schema: "ukps",
                table: "medicines_service_impacts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nhs_service_redesign_details",
                schema: "ukps",
                table: "medicines_service_impacts",
                type: "text",
                nullable: true);

            // ── medicines_product_details ────────────────────────────────────
            migrationBuilder.DropColumn(
                name: "indication_is_rare_disease",
                schema: "ukps",
                table: "medicines_product_details");

            migrationBuilder.DropColumn(
                name: "nice_ta_development_id",
                schema: "ukps",
                table: "medicines_product_details");

            migrationBuilder.AddColumn<int>(
                name: "therapeutic_area_id",
                schema: "ukps",
                table: "medicines_product_details",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_details_therapeutic_area_id",
                schema: "ukps",
                table: "medicines_product_details",
                column: "therapeutic_area_id");

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_product_details_therapeutic_areas_therapeutic_are",
                schema: "ukps",
                table: "medicines_product_details",
                column: "therapeutic_area_id",
                principalSchema: "ukps",
                principalTable: "therapeutic_areas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // ── Rename medicines_intl_recognitions → record_intl_recognitions
            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_medicines_intl_recognitions_regulatory_dates_intl_submissio
                    TO fk_record_intl_recognitions_regulatory_dates_intl_submission_d;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_medicines_intl_recognitions_regulatory_dates_intl_licence_d
                    TO fk_record_intl_recognitions_regulatory_dates_intl_licence_date;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_medicines_intl_recognitions_record_revisions_revision_id
                    TO fk_record_intl_recognitions_record_revisions_revision_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_medicines_intl_recognitions_irp_routes_irp_route_id
                    TO fk_record_intl_recognitions_irp_routes_irp_route_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT fk_medicines_intl_recognitions_irp_reference_regulators_irp_re
                    TO fk_record_intl_recognitions_irp_reference_regulators_irp_refer;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_intl_recognitions
                    RENAME CONSTRAINT pk_medicines_intl_recognitions
                    TO pk_record_intl_recognitions;
                """);

            migrationBuilder.RenameIndex(
                name: "ix_medicines_intl_recognitions_irp_route_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_record_intl_recognitions_irp_route_id");

            migrationBuilder.RenameIndex(
                name: "ix_medicines_intl_recognitions_irp_reference_regulator_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_record_intl_recognitions_irp_reference_regulator_id");

            migrationBuilder.RenameIndex(
                name: "ix_medicines_intl_recognitions_intl_submission_date_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_record_intl_recognitions_intl_submission_date_id");

            migrationBuilder.RenameIndex(
                name: "ix_medicines_intl_recognitions_intl_licence_date_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_record_intl_recognitions_intl_licence_date_id");

            migrationBuilder.RenameIndex(
                name: "ix_medicines_intl_recognition_revision_id",
                schema: "ukps",
                table: "medicines_intl_recognitions",
                newName: "ix_record_intl_recognition_revision_id");

            migrationBuilder.RenameTable(
                name: "medicines_intl_recognitions",
                schema: "ukps",
                newName: "record_intl_recognitions",
                newSchema: "ukps");

            // ── Rename medicines_global_submissions → record_global_submissions
            migrationBuilder.AddColumn<string>(
                name: "global_first_submission_notes",
                schema: "ukps",
                table: "medicines_global_submissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "global_submission_estimated_date_id",
                schema: "ukps",
                table: "medicines_global_submissions",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_global_submissions
                    RENAME CONSTRAINT fk_medicines_global_submissions_regulatory_dates_global_submis
                    TO fk_record_global_submissions_regulatory_dates_global_submissio;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_global_submissions
                    RENAME CONSTRAINT fk_medicines_global_submissions_record_revisions_revision_id
                    TO fk_record_global_submissions_record_revisions_revision_id;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE ukps.medicines_global_submissions
                    RENAME CONSTRAINT pk_medicines_global_submissions
                    TO pk_record_global_submissions;
                """);

            migrationBuilder.RenameIndex(
                name: "ix_medicines_global_submissions_global_submission_actual_date_",
                schema: "ukps",
                table: "medicines_global_submissions",
                newName: "ix_record_global_submissions_global_submission_actual_date_id");

            migrationBuilder.RenameIndex(
                name: "ix_medicines_global_submission_revision_id",
                schema: "ukps",
                table: "medicines_global_submissions",
                newName: "ix_record_global_submission_revision_id");

            migrationBuilder.RenameTable(
                name: "medicines_global_submissions",
                schema: "ukps",
                newName: "record_global_submissions",
                newSchema: "ukps");

            migrationBuilder.CreateIndex(
                name: "ix_record_global_submissions_global_submission_estimated_date_",
                schema: "ukps",
                table: "record_global_submissions",
                column: "global_submission_estimated_date_id");

            migrationBuilder.AddForeignKey(
                name: "fk_record_global_submissions_regulatory_dates_global_submissio1",
                schema: "ukps",
                table: "record_global_submissions",
                column: "global_submission_estimated_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // ── Recreate vaccines_populations ────────────────────────────────
            migrationBuilder.CreateTable(
                name: "vaccines_populations",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    age_group = table.Column<string>(type: "text", nullable: true),
                    risk_group = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_populations", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_populations_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_population_revision_id",
                schema: "ukps",
                table: "vaccines_populations",
                column: "revision_id",
                unique: true);

            // ── Recreate genomic_sample_type ─────────────────────────────────
            migrationBuilder.CreateTable(
                name: "genomic_sample_type",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_genomic_sample_type", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_medicines_laboratory_testings_genomic_sample_type_id",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                column: "genomic_sample_type_id");

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_laboratory_testings_genomic_sample_types_genomic_",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                column: "genomic_sample_type_id",
                principalSchema: "ukps",
                principalTable: "genomic_sample_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
