using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UKPS.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ukps");

            migrationBuilder.CreateTable(
                name: "app_user",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "text", nullable: false),
                    user_type = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    job_title = table.Column<string>(type: "text", nullable: true),
                    work_telephone = table.Column<string>(type: "text", nullable: true),
                    work_email = table.Column<string>(type: "text", nullable: true),
                    login_time = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    logout_time = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    last_active = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_user", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "atmp_classification",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_atmp_classification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bnf_chapter",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    code = table.Column<string>(type: "text", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bnf_chapter", x => x.id);
                    table.ForeignKey(
                        name: "fk_bnf_chapter_bnf_chapter_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "ukps",
                        principalTable: "bnf_chapter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "email_template",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    govnotify_template_id = table.Column<string>(type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    body = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "formulation_type",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formulation_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "genomic_sample_type",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_genomic_sample_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "irp_reference_regulator",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_irp_reference_regulator", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "irp_route",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_irp_route", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medicine_technology_status",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicine_technology_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mhra_procedure_type",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    relevant_to = table.Column<string>(type: "text", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mhra_procedure_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organisation",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_name = table.Column<string>(type: "text", nullable: false),
                    organisation_type = table.Column<string>(type: "text", nullable: false),
                    allowed_pharmaceutical_entity = table.Column<int>(type: "integer", nullable: false),
                    country_or_region = table.Column<string>(type: "text", nullable: true),
                    head_office_address = table.Column<string>(type: "text", nullable: true),
                    head_office_telephone = table.Column<string>(type: "text", nullable: true),
                    head_office_email = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    last_active = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pas_region",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pas_region", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "patient_pathway_point",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_patient_pathway_point", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "therapeutic_area",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_therapeutic_area", x => x.id);
                    table.ForeignKey(
                        name: "fk_therapeutic_area_therapeutic_area_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "ukps",
                        principalTable: "therapeutic_area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "uk_patient_population_range",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_uk_patient_population_range", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vaccine_administration_route",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_administration_route", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vaccine_disease_area",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_disease_area", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vaccine_platform",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_platform", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vaccine_storage_requirement",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_storage_requirement", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_preset",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    applicable_user_type = table.Column<string>(type: "text", nullable: false),
                    applicable_pharmaceutical_entity = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    configuration = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    is_shared = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_preset", x => x.id);
                    table.ForeignKey(
                        name: "fk_report_preset_app_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_audit",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: true),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_audit", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_audit_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_audit_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "email_audit",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    recipients = table.Column<string>(type: "text", nullable: false),
                    related_entity_type = table.Column<string>(type: "text", nullable: true),
                    related_entity_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_audit", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_audit_email_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "ukps",
                        principalTable: "email_template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organisation_audit",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: true),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_audit", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_audit_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_organisation_audit_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "terms_acceptance",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    relevant_pharmaceutical_entity = table.Column<int>(type: "integer", nullable: false),
                    signatory_name = table.Column<string>(type: "text", nullable: false),
                    signatory_email = table.Column<string>(type: "text", nullable: false),
                    signatory_job_title = table.Column<string>(type: "text", nullable: true),
                    link_expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    signed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_terms_acceptance", x => x.id);
                    table.ForeignKey(
                        name: "fk_terms_acceptance_organisation_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_org_membership",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    user_role = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    allowed_pharmaceutical_entity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_org_membership", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_org_membership_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_org_membership_organisation_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "report_audit",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    report_preset_id = table.Column<int>(type: "integer", nullable: true),
                    configuration = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    field_usage = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    result_count = table.Column<int>(type: "integer", nullable: true),
                    exported = table.Column<bool>(type: "boolean", nullable: true),
                    ran_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_audit", x => x.id);
                    table.ForeignKey(
                        name: "fk_report_audit_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_report_audit_report_presets_report_preset_id",
                        column: x => x.report_preset_id,
                        principalSchema: "ukps",
                        principalTable: "report_preset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicines_active_substance",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    medicines_product_detail_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_type = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_active_substance", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medicines_budget_impact",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    indication_specific_pricing_planned = table.Column<string>(type: "text", nullable: true),
                    indication_specific_pricing_details = table.Column<string>(type: "text", nullable: true),
                    net_uk_budget_impact_over5m = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_budget_impact", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medicines_pas_region",
                schema: "ukps",
                columns: table => new
                {
                    medicines_budget_impact_id = table.Column<int>(type: "integer", nullable: false),
                    pas_region_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_pas_region", x => new { x.medicines_budget_impact_id, x.pas_region_id });
                    table.ForeignKey(
                        name: "fk_medicines_pas_region_medicines_budget_impact_medicines_budg",
                        column: x => x.medicines_budget_impact_id,
                        principalSchema: "ukps",
                        principalTable: "medicines_budget_impact",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_medicines_pas_region_pas_regions_pas_region_id",
                        column: x => x.pas_region_id,
                        principalSchema: "ukps",
                        principalTable: "pas_region",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicines_company_info",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    is_originator_company = table.Column<string>(type: "text", nullable: true),
                    originator_company_name = table.Column<string>(type: "text", nullable: true),
                    is_co_marketed = table.Column<string>(type: "text", nullable: true),
                    co_marketing_company_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_company_info", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medicines_detail",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    mode_of_action = table.Column<string>(type: "text", nullable: true),
                    proposed_dose_regimen = table.Column<string>(type: "text", nullable: true),
                    is_personalised_medicine = table.Column<string>(type: "text", nullable: true),
                    is_repurposed_medicine = table.Column<string>(type: "text", nullable: true),
                    repurposed_medicine_details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_detail", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medicines_eams_pim",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    pim_designation_status = table.Column<string>(type: "text", nullable: true),
                    will_submit_to_eams = table.Column<string>(type: "text", nullable: true),
                    eams_opinion_decision = table.Column<string>(type: "text", nullable: true),
                    eams_submission_date_id = table.Column<int>(type: "integer", nullable: true),
                    eams_opinion_date_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_eams_pim", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medicines_eu_status",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    eu_orphan_status = table.Column<string>(type: "text", nullable: true),
                    eu_orphan_status_number = table.Column<string>(type: "text", nullable: true),
                    eu_orphan_granted_date_id = table.Column<int>(type: "integer", nullable: true),
                    eu_atmp_classification_status = table.Column<string>(type: "text", nullable: true),
                    atmp_classification_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_eu_status", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_eu_status_atmp_classifications_atmp_classificatio",
                        column: x => x.atmp_classification_id,
                        principalSchema: "ukps",
                        principalTable: "atmp_classification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicines_laboratory_testing",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    diagnostic_test_required = table.Column<string>(type: "text", nullable: true),
                    genomic_test_required = table.Column<string>(type: "text", nullable: true),
                    genomic_test_in_national_directory = table.Column<string>(type: "text", nullable: true),
                    national_genomic_test_directory_id = table.Column<string>(type: "text", nullable: true),
                    genomic_sample_type_id = table.Column<int>(type: "integer", nullable: true),
                    genomic_sample_type_other = table.Column<string>(type: "text", nullable: true),
                    genomic_turnaround_considerations = table.Column<string>(type: "text", nullable: true),
                    patient_pathway_point_id = table.Column<int>(type: "integer", nullable: true),
                    genomic_test_pathway_point_other = table.Column<string>(type: "text", nullable: true),
                    genomic_biomarker = table.Column<string>(type: "text", nullable: true),
                    genomic_alterations = table.Column<string>(type: "text", nullable: true),
                    genomic_test_used_in_trials = table.Column<string>(type: "text", nullable: true),
                    genomic_test_specificity_sensitivity = table.Column<string>(type: "text", nullable: true),
                    genomic_co_mutations = table.Column<string>(type: "text", nullable: true),
                    genomic_test_mandatory = table.Column<string>(type: "text", nullable: true),
                    genomic_test_notes = table.Column<string>(type: "text", nullable: true),
                    monitoring_tests_required = table.Column<string>(type: "text", nullable: true),
                    monitoring_tests_details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_laboratory_testing", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_laboratory_testing_genomic_sample_types_genomic_s",
                        column: x => x.genomic_sample_type_id,
                        principalSchema: "ukps",
                        principalTable: "genomic_sample_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_medicines_laboratory_testing_patient_pathway_points_patient",
                        column: x => x.patient_pathway_point_id,
                        principalSchema: "ukps",
                        principalTable: "patient_pathway_point",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicines_patient_identification",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    screening_required = table.Column<string>(type: "text", nullable: true),
                    screening_details = table.Column<string>(type: "text", nullable: true),
                    urgent_identification_required = table.Column<string>(type: "text", nullable: true),
                    urgent_identification_details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_patient_identification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "medicines_product_detail",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    record_title = table.Column<string>(type: "text", nullable: false),
                    branded_name = table.Column<string>(type: "text", nullable: true),
                    indication = table.Column<string>(type: "text", nullable: false),
                    indication_is_paediatric = table.Column<string>(type: "text", nullable: true),
                    indication_is_cancer = table.Column<string>(type: "text", nullable: true),
                    bnf_chapter_id = table.Column<int>(type: "integer", nullable: true),
                    therapeutic_area_id = table.Column<int>(type: "integer", nullable: true),
                    formulation_type_id = table.Column<int>(type: "integer", nullable: true),
                    presentation = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_product_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_product_detail_bnf_chapters_bnf_chapter_id",
                        column: x => x.bnf_chapter_id,
                        principalSchema: "ukps",
                        principalTable: "bnf_chapter",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_medicines_product_detail_formulation_types_formulation_type",
                        column: x => x.formulation_type_id,
                        principalSchema: "ukps",
                        principalTable: "formulation_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_medicines_product_detail_therapeutic_areas_therapeutic_area",
                        column: x => x.therapeutic_area_id,
                        principalSchema: "ukps",
                        principalTable: "therapeutic_area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicines_record_status",
                schema: "ukps",
                columns: table => new
                {
                    medicines_product_detail_id = table.Column<int>(type: "integer", nullable: false),
                    medicine_status_type_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_record_status", x => new { x.medicines_product_detail_id, x.medicine_status_type_id });
                    table.ForeignKey(
                        name: "fk_medicines_record_status_medicine_technology_statuses_medici",
                        column: x => x.medicine_status_type_id,
                        principalSchema: "ukps",
                        principalTable: "medicine_technology_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_medicines_record_status_medicines_product_detail_medicines_",
                        column: x => x.medicines_product_detail_id,
                        principalSchema: "ukps",
                        principalTable: "medicines_product_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicines_service_impact",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    existing_nhs_service = table.Column<string>(type: "text", nullable: true),
                    nhs_service_redesign_details = table.Column<string>(type: "text", nullable: true),
                    uk_patient_population_range_id = table.Column<int>(type: "integer", nullable: true),
                    uk_patient_population_notes = table.Column<string>(type: "text", nullable: true),
                    estimated_eligible_patient_population = table.Column<string>(type: "text", nullable: true),
                    compassionate_access_available = table.Column<string>(type: "text", nullable: true),
                    compassionate_access_details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_service_impact", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_service_impact_uk_patient_population_ranges_uk_pa",
                        column: x => x.uk_patient_population_range_id,
                        principalSchema: "ukps",
                        principalTable: "uk_patient_population_range",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "medicines_treatment_detail",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    proposed_place_in_therapy = table.Column<string>(type: "text", nullable: false),
                    estimated_duration_of_treatment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_treatment_detail", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "other_clinical_trial_number",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    clinical_trial_id = table.Column<int>(type: "integer", nullable: false),
                    other_registry_number = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_other_clinical_trial_number", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "qa_review",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    major_revision_submission_round_no = table.Column<int>(type: "integer", nullable: false),
                    outcome = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    reviewed_by = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qa_review", x => x.id);
                    table.ForeignKey(
                        name: "fk_qa_review_app_user_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "qa_review_item",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    qa_review_id = table.Column<int>(type: "integer", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: false),
                    issue_type = table.Column<string>(type: "text", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    resolution_status = table.Column<string>(type: "text", nullable: false),
                    resolved_by = table.Column<int>(type: "integer", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qa_review_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_qa_review_item_app_user_resolved_by",
                        column: x => x.resolved_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_qa_review_item_qa_review_qa_review_id",
                        column: x => x.qa_review_id,
                        principalSchema: "ukps",
                        principalTable: "qa_review",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    record_type = table.Column<string>(type: "text", nullable: false),
                    record_status = table.Column<string>(type: "text", nullable: false),
                    published_revision_id = table.Column<int>(type: "integer", nullable: true),
                    current_draft_revision_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_app_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_organisation_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_revision",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    record_id = table.Column<int>(type: "integer", nullable: false),
                    based_on_revision_id = table.Column<int>(type: "integer", nullable: true),
                    revision_no = table.Column<int>(type: "integer", nullable: false),
                    major_version = table.Column<int>(type: "integer", nullable: false),
                    minor_version = table.Column<int>(type: "integer", nullable: false),
                    workflow_status = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    submitted_by = table.Column<int>(type: "integer", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_revision", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_revision_app_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_revision_app_user_submitted_by",
                        column: x => x.submitted_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_revision_app_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_revision_record_record_id",
                        column: x => x.record_id,
                        principalSchema: "ukps",
                        principalTable: "record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_revision_record_revision_based_on_revision_id",
                        column: x => x.based_on_revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_status_history",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    record_id = table.Column<int>(type: "integer", nullable: false),
                    from_status = table.Column<string>(type: "text", nullable: true),
                    to_status = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_status_history_app_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_status_history_record_record_id",
                        column: x => x.record_id,
                        principalSchema: "ukps",
                        principalTable: "record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_watchlist",
                schema: "ukps",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    record_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_watchlist", x => new { x.user_id, x.record_id });
                    table.ForeignKey(
                        name: "fk_record_watchlist_app_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_watchlist_record_record_id",
                        column: x => x.record_id,
                        principalSchema: "ukps",
                        principalTable: "record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_clinical_trial",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    study_name = table.Column<string>(type: "text", nullable: false),
                    clinical_trials_gov_number = table.Column<string>(type: "text", nullable: true),
                    brief_description = table.Column<string>(type: "text", nullable: true),
                    recruiting_in_uk = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_clinical_trial", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_clinical_trial_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_event",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    record_id = table.Column<int>(type: "integer", nullable: false),
                    revision_id = table.Column<int>(type: "integer", nullable: true),
                    qa_review_id = table.Column<int>(type: "integer", nullable: true),
                    qa_review_item_id = table.Column<int>(type: "integer", nullable: true),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    performed_by = table.Column<int>(type: "integer", nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    payload = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_event", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_event_app_user_performed_by",
                        column: x => x.performed_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_event_qa_review_item_qa_review_item_id",
                        column: x => x.qa_review_item_id,
                        principalSchema: "ukps",
                        principalTable: "qa_review_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_event_qa_review_qa_review_id",
                        column: x => x.qa_review_id,
                        principalSchema: "ukps",
                        principalTable: "qa_review",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_event_record_record_id",
                        column: x => x.record_id,
                        principalSchema: "ukps",
                        principalTable: "record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_event_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_hta",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    hta_body_vaccine = table.Column<string>(type: "text", nullable: true),
                    hta_nice_aligned_pathway = table.Column<string>(type: "text", nullable: true),
                    hta_additional_details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_hta", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_hta_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_mhra_procedure",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    mhra_procedure_type_id = table.Column<int>(type: "integer", nullable: true),
                    procedure_details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_mhra_procedure", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_mhra_procedure_mhra_procedure_type_mhra_procedure_ty",
                        column: x => x.mhra_procedure_type_id,
                        principalSchema: "ukps",
                        principalTable: "mhra_procedure_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_mhra_procedure_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "regulatory_date",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    date_event = table.Column<string>(type: "text", nullable: false),
                    date_precision = table.Column<string>(type: "text", nullable: false),
                    date_value = table.Column<DateOnly>(type: "date", nullable: false),
                    is_confidential = table.Column<bool>(type: "boolean", nullable: false),
                    conditional_approval_anticipated = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regulatory_date", x => x.id);
                    table.ForeignKey(
                        name: "fk_regulatory_date_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_company_info",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    is_originator_company = table.Column<string>(type: "text", nullable: true),
                    originator_company_name = table.Column<string>(type: "text", nullable: true),
                    has_been_acquired = table.Column<string>(type: "text", nullable: false),
                    previous_owner = table.Column<string>(type: "text", nullable: true),
                    has_grant_funding = table.Column<string>(type: "text", nullable: false),
                    grant_funding_identifier = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_company_info", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_company_info_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_disease_detail",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    disease_area_id = table.Column<int>(type: "integer", nullable: true),
                    disease_target = table.Column<string>(type: "text", nullable: false),
                    age_group = table.Column<string>(type: "text", nullable: false),
                    risk_group = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_disease_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_disease_detail_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vaccines_disease_detail_vaccine_disease_area_disease_area_id",
                        column: x => x.disease_area_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_disease_area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_population",
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
                    table.PrimaryKey("pk_vaccines_population", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_population_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_product_detail",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    record_title = table.Column<string>(type: "text", nullable: false),
                    company_code = table.Column<string>(type: "text", nullable: false),
                    branded_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_product_detail", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_product_detail_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_service_readiness",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    storage_requirement_id = table.Column<int>(type: "integer", nullable: false),
                    requires_reconstitution = table.Column<string>(type: "text", nullable: false),
                    additional_service_notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_service_readiness", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_service_readiness_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vaccines_service_readiness_vaccine_storage_requirement_stor",
                        column: x => x.storage_requirement_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_storage_requirement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_technology",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    vaccine_platform_id = table.Column<int>(type: "integer", nullable: false),
                    vaccine_platform_other = table.Column<string>(type: "text", nullable: true),
                    administration_route_id = table.Column<int>(type: "integer", nullable: false),
                    has_adjuvant = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_technology", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_technology_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vaccines_technology_vaccine_administration_route_administra",
                        column: x => x.administration_route_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_administration_route",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vaccines_technology_vaccine_platform_vaccine_platform_id",
                        column: x => x.vaccine_platform_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_platform",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_event_field_change",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    record_event_id = table.Column<int>(type: "integer", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: false),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_event_field_change", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_event_field_change_record_event_record_event_id",
                        column: x => x.record_event_id,
                        principalSchema: "ukps",
                        principalTable: "record_event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_hta_body",
                schema: "ukps",
                columns: table => new
                {
                    record_hta_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_hta_body", x => new { x.record_hta_id, x.label });
                    table.ForeignKey(
                        name: "fk_record_hta_body_record_htas_record_hta_id",
                        column: x => x.record_hta_id,
                        principalSchema: "ukps",
                        principalTable: "record_hta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_global_submission",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    global_first_submission_region = table.Column<string>(type: "text", nullable: true),
                    global_first_submission_notes = table.Column<string>(type: "text", nullable: true),
                    global_submission_estimated_date_id = table.Column<int>(type: "integer", nullable: true),
                    global_submission_actual_date_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_global_submission", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_global_submission_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_global_submission_regulatory_dates_global_submission",
                        column: x => x.global_submission_actual_date_id,
                        principalSchema: "ukps",
                        principalTable: "regulatory_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_global_submission_regulatory_dates_global_submission1",
                        column: x => x.global_submission_estimated_date_id,
                        principalSchema: "ukps",
                        principalTable: "regulatory_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_intl_recognition",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    irp_reference_regulator_id = table.Column<int>(type: "integer", nullable: true),
                    irp_route_id = table.Column<int>(type: "integer", nullable: true),
                    intl_conditional_approval_anticipated = table.Column<string>(type: "text", nullable: true),
                    intl_submission_date_id = table.Column<int>(type: "integer", nullable: true),
                    intl_licence_date_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_intl_recognition", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_intl_recognition_irp_reference_regulator_irp_referen",
                        column: x => x.irp_reference_regulator_id,
                        principalSchema: "ukps",
                        principalTable: "irp_reference_regulator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_intl_recognition_irp_route_irp_route_id",
                        column: x => x.irp_route_id,
                        principalSchema: "ukps",
                        principalTable: "irp_route",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_intl_recognition_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_intl_recognition_regulatory_dates_intl_licence_date_",
                        column: x => x.intl_licence_date_id,
                        principalSchema: "ukps",
                        principalTable: "regulatory_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_intl_recognition_regulatory_dates_intl_submission_da",
                        column: x => x.intl_submission_date_id,
                        principalSchema: "ukps",
                        principalTable: "regulatory_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "record_mhra_date",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    uk_submission_date_id = table.Column<int>(type: "integer", nullable: true),
                    uk_licence_date_id = table.Column<int>(type: "integer", nullable: true),
                    uk_launch_date_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_mhra_date", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_mhra_date_record_revision_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revision",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_mhra_date_regulatory_dates_uk_launch_date_id",
                        column: x => x.uk_launch_date_id,
                        principalSchema: "ukps",
                        principalTable: "regulatory_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_mhra_date_regulatory_dates_uk_licence_date_id",
                        column: x => x.uk_licence_date_id,
                        principalSchema: "ukps",
                        principalTable: "regulatory_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_record_mhra_date_regulatory_dates_uk_submission_date_id",
                        column: x => x.uk_submission_date_id,
                        principalSchema: "ukps",
                        principalTable: "regulatory_date",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_pathogen",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vaccines_disease_detail_id = table.Column<int>(type: "integer", nullable: false),
                    pathogen_name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_pathogen", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_pathogen_vaccines_disease_detail_vaccines_disease_",
                        column: x => x.vaccines_disease_detail_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_disease_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_company_code",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vaccines_product_detail_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_company_code", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_company_code_vaccines_product_details_vaccines_pro",
                        column: x => x.vaccines_product_detail_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_product_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_adjuvant",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vaccines_technology_id = table.Column<int>(type: "integer", nullable: false),
                    adjuvant_name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_adjuvant", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_adjuvant_vaccines_technologies_vaccines_technology",
                        column: x => x.vaccines_technology_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_technology",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vaccines_antigen",
                schema: "ukps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vaccines_technology_id = table.Column<int>(type: "integer", nullable: false),
                    antigen_name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_antigen", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_antigen_vaccines_technologies_vaccines_technology_",
                        column: x => x.vaccines_technology_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_technology",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_app_user_username",
                schema: "ukps",
                table: "app_user",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bnf_chapter_parent_id",
                schema: "ukps",
                table: "bnf_chapter",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_email_audit_template_id",
                schema: "ukps",
                table: "email_audit",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_active_substance_product_detail_id",
                schema: "ukps",
                table: "medicines_active_substance",
                column: "medicines_product_detail_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_budget_impact_revision_id",
                schema: "ukps",
                table: "medicines_budget_impact",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_company_info_revision_id",
                schema: "ukps",
                table: "medicines_company_info",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_detail_revision_id",
                schema: "ukps",
                table: "medicines_detail",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eams_pim_eams_opinion_date_id",
                schema: "ukps",
                table: "medicines_eams_pim",
                column: "eams_opinion_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eams_pim_eams_submission_date_id",
                schema: "ukps",
                table: "medicines_eams_pim",
                column: "eams_submission_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eams_pim_revision_id",
                schema: "ukps",
                table: "medicines_eams_pim",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eu_status_atmp_classification_id",
                schema: "ukps",
                table: "medicines_eu_status",
                column: "atmp_classification_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eu_status_eu_orphan_granted_date_id",
                schema: "ukps",
                table: "medicines_eu_status",
                column: "eu_orphan_granted_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eu_status_revision_id",
                schema: "ukps",
                table: "medicines_eu_status",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_laboratory_testing_genomic_sample_type_id",
                schema: "ukps",
                table: "medicines_laboratory_testing",
                column: "genomic_sample_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_laboratory_testing_patient_pathway_point_id",
                schema: "ukps",
                table: "medicines_laboratory_testing",
                column: "patient_pathway_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_laboratory_testing_revision_id",
                schema: "ukps",
                table: "medicines_laboratory_testing",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_pas_region_pas_region_id",
                schema: "ukps",
                table: "medicines_pas_region",
                column: "pas_region_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_patient_identification_revision_id",
                schema: "ukps",
                table: "medicines_patient_identification",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_detail_bnf_chapter_id",
                schema: "ukps",
                table: "medicines_product_detail",
                column: "bnf_chapter_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_detail_formulation_type_id",
                schema: "ukps",
                table: "medicines_product_detail",
                column: "formulation_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_detail_revision_id",
                schema: "ukps",
                table: "medicines_product_detail",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_detail_therapeutic_area_id",
                schema: "ukps",
                table: "medicines_product_detail",
                column: "therapeutic_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_record_status_medicine_status_type_id",
                schema: "ukps",
                table: "medicines_record_status",
                column: "medicine_status_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_service_impact_revision_id",
                schema: "ukps",
                table: "medicines_service_impact",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_medicines_service_impact_uk_patient_population_range_id",
                schema: "ukps",
                table: "medicines_service_impact",
                column: "uk_patient_population_range_id");

            migrationBuilder.CreateIndex(
                name: "ix_medicines_treatment_detail_revision_id",
                schema: "ukps",
                table: "medicines_treatment_detail",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_organisation_audit_organisation_id",
                schema: "ukps",
                table: "organisation_audit",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_organisation_audit_updated_by",
                schema: "ukps",
                table: "organisation_audit",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_other_clinical_trial_number_clinical_trial_id",
                schema: "ukps",
                table: "other_clinical_trial_number",
                column: "clinical_trial_id");

            migrationBuilder.CreateIndex(
                name: "ix_qa_review_reviewed_by",
                schema: "ukps",
                table: "qa_review",
                column: "reviewed_by");

            migrationBuilder.CreateIndex(
                name: "ix_qa_review_revision_id",
                schema: "ukps",
                table: "qa_review",
                column: "revision_id");

            migrationBuilder.CreateIndex(
                name: "ix_qa_review_item_qa_review_id",
                schema: "ukps",
                table: "qa_review_item",
                column: "qa_review_id");

            migrationBuilder.CreateIndex(
                name: "ix_qa_review_item_resolved_by",
                schema: "ukps",
                table: "qa_review_item",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "ix_record_created_by",
                schema: "ukps",
                table: "record",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_record_current_draft_revision_id",
                schema: "ukps",
                table: "record",
                column: "current_draft_revision_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_organisation_id",
                schema: "ukps",
                table: "record",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_published_revision_id",
                schema: "ukps",
                table: "record",
                column: "published_revision_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_type_status_reviewed_at",
                schema: "ukps",
                table: "record",
                columns: new[] { "record_type", "record_status", "reviewed_at" });

            migrationBuilder.CreateIndex(
                name: "ix_record_clinical_trial_revision_id",
                schema: "ukps",
                table: "record_clinical_trial",
                column: "revision_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_event_performed_by",
                schema: "ukps",
                table: "record_event",
                column: "performed_by");

            migrationBuilder.CreateIndex(
                name: "ix_record_event_qa_review_id",
                schema: "ukps",
                table: "record_event",
                column: "qa_review_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_event_qa_review_item_id",
                schema: "ukps",
                table: "record_event",
                column: "qa_review_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_event_record_id_event_type",
                schema: "ukps",
                table: "record_event",
                columns: new[] { "record_id", "event_type" });

            migrationBuilder.CreateIndex(
                name: "ix_record_event_revision_id",
                schema: "ukps",
                table: "record_event",
                column: "revision_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_event_field_change_record_event_id",
                schema: "ukps",
                table: "record_event_field_change",
                column: "record_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_global_submission_global_submission_actual_date_id",
                schema: "ukps",
                table: "record_global_submission",
                column: "global_submission_actual_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_global_submission_global_submission_estimated_date_id",
                schema: "ukps",
                table: "record_global_submission",
                column: "global_submission_estimated_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_global_submission_revision_id",
                schema: "ukps",
                table: "record_global_submission",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_record_hta_revision_id",
                schema: "ukps",
                table: "record_hta",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognition_intl_licence_date_id",
                schema: "ukps",
                table: "record_intl_recognition",
                column: "intl_licence_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognition_intl_submission_date_id",
                schema: "ukps",
                table: "record_intl_recognition",
                column: "intl_submission_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognition_irp_reference_regulator_id",
                schema: "ukps",
                table: "record_intl_recognition",
                column: "irp_reference_regulator_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognition_irp_route_id",
                schema: "ukps",
                table: "record_intl_recognition",
                column: "irp_route_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognition_revision_id",
                schema: "ukps",
                table: "record_intl_recognition",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_date_revision_id",
                schema: "ukps",
                table: "record_mhra_date",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_date_uk_launch_date_id",
                schema: "ukps",
                table: "record_mhra_date",
                column: "uk_launch_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_date_uk_licence_date_id",
                schema: "ukps",
                table: "record_mhra_date",
                column: "uk_licence_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_date_uk_submission_date_id",
                schema: "ukps",
                table: "record_mhra_date",
                column: "uk_submission_date_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_procedure_mhra_procedure_type_id",
                schema: "ukps",
                table: "record_mhra_procedure",
                column: "mhra_procedure_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_procedure_revision_id",
                schema: "ukps",
                table: "record_mhra_procedure",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_based_on_revision_id",
                schema: "ukps",
                table: "record_revision",
                column: "based_on_revision_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_created_by",
                schema: "ukps",
                table: "record_revision",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_record_id_major_minor",
                schema: "ukps",
                table: "record_revision",
                columns: new[] { "record_id", "major_version", "minor_version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_record_id_revision_no",
                schema: "ukps",
                table: "record_revision",
                columns: new[] { "record_id", "revision_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_submitted_by",
                schema: "ukps",
                table: "record_revision",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_updated_by",
                schema: "ukps",
                table: "record_revision",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_record_status_history_record_id",
                schema: "ukps",
                table: "record_status_history",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "ix_record_status_history_updated_by",
                schema: "ukps",
                table: "record_status_history",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_record_watchlist_record_id",
                schema: "ukps",
                table: "record_watchlist",
                column: "record_id");

            migrationBuilder.CreateIndex(
                name: "ix_regulatory_date_revision_event_precision",
                schema: "ukps",
                table: "regulatory_date",
                columns: new[] { "revision_id", "date_event", "date_precision" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_report_audit_report_preset_id",
                schema: "ukps",
                table: "report_audit",
                column: "report_preset_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_audit_user_id",
                schema: "ukps",
                table: "report_audit",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_report_preset_created_by",
                schema: "ukps",
                table: "report_preset",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "ix_terms_acceptance_organisation_id",
                schema: "ukps",
                table: "terms_acceptance",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_therapeutic_area_parent_id",
                schema: "ukps",
                table: "therapeutic_area",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_audit_updated_by",
                schema: "ukps",
                table: "user_audit",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "ix_user_audit_user_id",
                schema: "ukps",
                table: "user_audit",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_org_membership_organisation_id",
                schema: "ukps",
                table: "user_org_membership",
                column: "organisation_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_org_membership_user_org_entity",
                schema: "ukps",
                table: "user_org_membership",
                columns: new[] { "user_id", "organisation_id", "allowed_pharmaceutical_entity" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_adjuvant_technology_id",
                schema: "ukps",
                table: "vaccines_adjuvant",
                column: "vaccines_technology_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_antigen_technology_id",
                schema: "ukps",
                table: "vaccines_antigen",
                column: "vaccines_technology_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_company_code_product_detail_id",
                schema: "ukps",
                table: "vaccines_company_code",
                column: "vaccines_product_detail_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_company_info_revision_id",
                schema: "ukps",
                table: "vaccines_company_info",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_disease_detail_disease_area_id",
                schema: "ukps",
                table: "vaccines_disease_detail",
                column: "disease_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_disease_detail_revision_id",
                schema: "ukps",
                table: "vaccines_disease_detail",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_pathogen_disease_detail_id",
                schema: "ukps",
                table: "vaccines_pathogen",
                column: "vaccines_disease_detail_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_population_revision_id",
                schema: "ukps",
                table: "vaccines_population",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_product_detail_revision_id",
                schema: "ukps",
                table: "vaccines_product_detail",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_service_readiness_revision_id",
                schema: "ukps",
                table: "vaccines_service_readiness",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_service_readiness_storage_requirement_id",
                schema: "ukps",
                table: "vaccines_service_readiness",
                column: "storage_requirement_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_technology_administration_route_id",
                schema: "ukps",
                table: "vaccines_technology",
                column: "administration_route_id");

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_technology_revision_id",
                schema: "ukps",
                table: "vaccines_technology",
                column: "revision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_technology_vaccine_platform_id",
                schema: "ukps",
                table: "vaccines_technology",
                column: "vaccine_platform_id");

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_active_substance_medicines_product_details_medici",
                schema: "ukps",
                table: "medicines_active_substance",
                column: "medicines_product_detail_id",
                principalSchema: "ukps",
                principalTable: "medicines_product_detail",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_budget_impact_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_budget_impact",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_company_info_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_company_info",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_detail_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_detail",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eams_pim_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_eams_pim",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eams_pim_regulatory_dates_eams_opinion_date_id",
                schema: "ukps",
                table: "medicines_eams_pim",
                column: "eams_opinion_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eams_pim_regulatory_dates_eams_submission_date_id",
                schema: "ukps",
                table: "medicines_eams_pim",
                column: "eams_submission_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eu_status_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_eu_status",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eu_status_regulatory_dates_eu_orphan_granted_date",
                schema: "ukps",
                table: "medicines_eu_status",
                column: "eu_orphan_granted_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_date",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_laboratory_testing_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_laboratory_testing",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_patient_identification_record_revisions_revision_",
                schema: "ukps",
                table: "medicines_patient_identification",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_product_detail_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_product_detail",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_service_impact_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_service_impact",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_treatment_detail_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_treatment_detail",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_other_clinical_trial_number_record_clinical_trials_clinical",
                schema: "ukps",
                table: "other_clinical_trial_number",
                column: "clinical_trial_id",
                principalSchema: "ukps",
                principalTable: "record_clinical_trial",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_qa_review_record_revisions_revision_id",
                schema: "ukps",
                table: "qa_review",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_record_record_revisions_current_draft_revision_id",
                schema: "ukps",
                table: "record",
                column: "current_draft_revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_record_record_revisions_published_revision_id",
                schema: "ukps",
                table: "record",
                column: "published_revision_id",
                principalSchema: "ukps",
                principalTable: "record_revision",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_record_record_revisions_current_draft_revision_id",
                schema: "ukps",
                table: "record");

            migrationBuilder.DropForeignKey(
                name: "fk_record_record_revisions_published_revision_id",
                schema: "ukps",
                table: "record");

            migrationBuilder.DropTable(
                name: "email_audit",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_active_substance",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_company_info",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_detail",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_eams_pim",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_eu_status",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_laboratory_testing",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_pas_region",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_patient_identification",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_record_status",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_service_impact",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_treatment_detail",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "organisation_audit",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "other_clinical_trial_number",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_event_field_change",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_global_submission",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_hta_body",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_intl_recognition",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_mhra_date",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_mhra_procedure",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_status_history",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_watchlist",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "report_audit",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "terms_acceptance",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "user_audit",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "user_org_membership",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_adjuvant",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_antigen",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_company_code",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_company_info",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_pathogen",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_population",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_service_readiness",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "email_template",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "atmp_classification",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "genomic_sample_type",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "patient_pathway_point",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_budget_impact",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "pas_region",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicine_technology_status",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "medicines_product_detail",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "uk_patient_population_range",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_clinical_trial",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_event",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_hta",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "irp_reference_regulator",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "irp_route",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "regulatory_date",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "mhra_procedure_type",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "report_preset",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_technology",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_product_detail",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccines_disease_detail",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccine_storage_requirement",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "bnf_chapter",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "formulation_type",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "therapeutic_area",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "qa_review_item",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccine_administration_route",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccine_platform",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "vaccine_disease_area",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "qa_review",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record_revision",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "record",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "app_user",
                schema: "ukps");

            migrationBuilder.DropTable(
                name: "organisation",
                schema: "ukps");
        }
    }
}
