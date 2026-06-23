using System;
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
            migrationBuilder.EnsureSchema(name: "ukps");

            migrationBuilder.CreateTable(
                name: "app_user",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    username = table.Column<string>(type: "text", nullable: false),
                    user_type = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    job_title = table.Column<string>(type: "text", nullable: true),
                    work_telephone = table.Column<string>(type: "text", nullable: true),
                    work_email = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_app_user", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "atmp_classification",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_atmp_classification", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "bnf_chapters",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    code = table.Column<string>(type: "text", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bnf_chapters", x => x.id);
                    table.ForeignKey(
                        name: "fk_bnf_chapters_bnf_chapters_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "ukps",
                        principalTable: "bnf_chapters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "email_templates",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    govnotify_template_id = table.Column<string>(type: "text", nullable: true),
                    subject = table.Column<string>(type: "text", nullable: true),
                    body = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_templates", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "formulation_type",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_formulation_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "genomic_sample_type",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_genomic_sample_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "irp_reference_regulator",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_irp_reference_regulator", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "irp_route",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_irp_route", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "medicine_technology_status",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicine_technology_status", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "mhra_procedure_type",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    relevant_to = table.Column<int>(type: "integer", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mhra_procedure_type", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "organisations",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    organisation_name = table.Column<string>(type: "text", nullable: false),
                    organisation_type = table.Column<int>(type: "integer", nullable: false),
                    allowed_pharmaceutical_entity = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    country_or_region = table.Column<string>(type: "text", nullable: true),
                    head_office_address = table.Column<string>(type: "text", nullable: true),
                    head_office_telephone = table.Column<string>(type: "text", nullable: true),
                    head_office_email = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    last_active = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisations", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "pas_region",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pas_region", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "patient_pathway_point",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_patient_pathway_point", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "therapeutic_areas",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    parent_id = table.Column<int>(type: "integer", nullable: true),
                    label = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_therapeutic_areas", x => x.id);
                    table.ForeignKey(
                        name: "fk_therapeutic_areas_therapeutic_areas_parent_id",
                        column: x => x.parent_id,
                        principalSchema: "ukps",
                        principalTable: "therapeutic_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "uk_patient_population_range",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_uk_patient_population_range", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccine_administration_route",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_administration_route", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccine_disease_area",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_disease_area", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccine_platform",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_platform", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccine_storage_requirement",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    label = table.Column<string>(type: "text", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccine_storage_requirement", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "report_presets",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    applicable_user_type = table.Column<int>(type: "integer", nullable: false),
                    applicable_pharmaceutical_entity = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    title = table.Column<string>(type: "text", nullable: false),
                    is_shared = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    configuration = table.Column<string>(type: "jsonb", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_presets", x => x.id);
                    table.ForeignKey(
                        name: "fk_report_presets_app_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_audits",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: true),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_audits", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_audits_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_user_audits_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "email_audits",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    template_id = table.Column<int>(type: "integer", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    recipients = table.Column<string>(type: "text", nullable: false),
                    related_entity_type = table.Column<string>(type: "text", nullable: true),
                    related_entity_id = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_email_audits", x => x.id);
                    table.ForeignKey(
                        name: "fk_email_audits_email_templates_template_id",
                        column: x => x.template_id,
                        principalSchema: "ukps",
                        principalTable: "email_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "organisation_audits",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: true),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_organisation_audits", x => x.id);
                    table.ForeignKey(
                        name: "fk_organisation_audits_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_organisation_audits_users_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "terms_acceptances",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    relevant_pharmaceutical_entity = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    signatory_name = table.Column<string>(type: "text", nullable: false),
                    signatory_email = table.Column<string>(type: "text", nullable: false),
                    signatory_job_title = table.Column<string>(type: "text", nullable: true),
                    link_expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    signed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    ip_address = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_terms_acceptances", x => x.id);
                    table.ForeignKey(
                        name: "fk_terms_acceptances_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "user_org_memberships",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    user_role = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    allowed_pharmaceutical_entity = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_org_memberships", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_org_memberships_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_user_org_memberships_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "report_audits",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    report_preset_id = table.Column<int>(type: "integer", nullable: true),
                    result_count = table.Column<int>(type: "integer", nullable: true),
                    exported = table.Column<bool>(type: "boolean", nullable: true),
                    ran_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    configuration = table.Column<string>(type: "jsonb", nullable: false),
                    field_usage = table.Column<string>(type: "jsonb", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_report_audits", x => x.id);
                    table.ForeignKey(
                        name: "fk_report_audits_report_presets_report_preset_id",
                        column: x => x.report_preset_id,
                        principalSchema: "ukps",
                        principalTable: "report_presets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_report_audits_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_active_substances",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    medicines_product_detail_id = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    name = table.Column<string>(type: "text", nullable: false),
                    name_type = table.Column<int>(type: "integer", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_active_substances", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_budget_impacts",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    indication_specific_pricing_planned = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    indication_specific_pricing_details = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    net_uk_budget_impact_over5m = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_budget_impacts", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_pas_regions",
                schema: "ukps",
                columns: table => new
                {
                    medicines_budget_impact_id = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    pas_region_id = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_medicines_pas_regions",
                        x => new { x.medicines_budget_impact_id, x.pas_region_id }
                    );
                    table.ForeignKey(
                        name: "fk_medicines_pas_regions_medicines_budget_impacts_medicines_bu",
                        column: x => x.medicines_budget_impact_id,
                        principalSchema: "ukps",
                        principalTable: "medicines_budget_impacts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_medicines_pas_regions_pas_regions_pas_region_id",
                        column: x => x.pas_region_id,
                        principalSchema: "ukps",
                        principalTable: "pas_region",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_company_infos",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    is_originator_company = table.Column<int>(type: "integer", nullable: true),
                    originator_company_name = table.Column<string>(type: "text", nullable: true),
                    is_co_marketed = table.Column<int>(type: "integer", nullable: true),
                    co_marketing_company_name = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_company_infos", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_details",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    mode_of_action = table.Column<string>(type: "text", nullable: true),
                    proposed_dose_regimen = table.Column<string>(type: "text", nullable: true),
                    is_personalised_medicine = table.Column<int>(type: "integer", nullable: true),
                    is_repurposed_medicine = table.Column<int>(type: "integer", nullable: true),
                    repurposed_medicine_details = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_details", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_eams_pims",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    pim_designation_status = table.Column<int>(type: "integer", nullable: true),
                    will_submit_to_eams = table.Column<int>(type: "integer", nullable: true),
                    eams_opinion_decision = table.Column<int>(type: "integer", nullable: true),
                    eams_submission_date_id = table.Column<int>(type: "integer", nullable: true),
                    eams_opinion_date_id = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_eams_pims", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_eu_statuses",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    eu_orphan_status = table.Column<int>(type: "integer", nullable: true),
                    eu_orphan_status_number = table.Column<string>(type: "text", nullable: true),
                    eu_orphan_granted_date_id = table.Column<int>(type: "integer", nullable: true),
                    eu_atmp_classification_status = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    atmp_classification_id = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_eu_statuses", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_eu_statuses_atmp_classifications_atmp_classificat",
                        column: x => x.atmp_classification_id,
                        principalSchema: "ukps",
                        principalTable: "atmp_classification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_laboratory_testings",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    diagnostic_test_required = table.Column<int>(type: "integer", nullable: true),
                    genomic_test_required = table.Column<int>(type: "integer", nullable: true),
                    genomic_test_in_national_directory = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    national_genomic_test_directory_id = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    genomic_sample_type_id = table.Column<int>(type: "integer", nullable: true),
                    genomic_sample_type_other = table.Column<string>(type: "text", nullable: true),
                    genomic_turnaround_considerations = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    patient_pathway_point_id = table.Column<int>(type: "integer", nullable: true),
                    genomic_test_pathway_point_other = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    genomic_biomarker = table.Column<string>(type: "text", nullable: true),
                    genomic_alterations = table.Column<string>(type: "text", nullable: true),
                    genomic_test_used_in_trials = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    genomic_test_specificity_sensitivity = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    genomic_co_mutations = table.Column<string>(type: "text", nullable: true),
                    genomic_test_mandatory = table.Column<int>(type: "integer", nullable: true),
                    genomic_test_notes = table.Column<string>(type: "text", nullable: true),
                    monitoring_tests_required = table.Column<int>(type: "integer", nullable: true),
                    monitoring_tests_details = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_laboratory_testings", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_laboratory_testings_genomic_sample_types_genomic_",
                        column: x => x.genomic_sample_type_id,
                        principalSchema: "ukps",
                        principalTable: "genomic_sample_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_medicines_laboratory_testings_patient_pathway_points_patien",
                        column: x => x.patient_pathway_point_id,
                        principalSchema: "ukps",
                        principalTable: "patient_pathway_point",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_patient_identifications",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    screening_required = table.Column<int>(type: "integer", nullable: true),
                    screening_details = table.Column<string>(type: "text", nullable: true),
                    urgent_identification_required = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    urgent_identification_details = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_patient_identifications", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_product_details",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    record_title = table.Column<string>(type: "text", nullable: false),
                    branded_name = table.Column<string>(type: "text", nullable: true),
                    indication = table.Column<string>(type: "text", nullable: false),
                    indication_is_paediatric = table.Column<int>(type: "integer", nullable: true),
                    indication_is_cancer = table.Column<int>(type: "integer", nullable: true),
                    bnf_chapter_id = table.Column<int>(type: "integer", nullable: true),
                    therapeutic_area_id = table.Column<int>(type: "integer", nullable: true),
                    formulation_type_id = table.Column<int>(type: "integer", nullable: true),
                    presentation = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_product_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_product_details_bnf_chapters_bnf_chapter_id",
                        column: x => x.bnf_chapter_id,
                        principalSchema: "ukps",
                        principalTable: "bnf_chapters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_medicines_product_details_formulation_types_formulation_typ",
                        column: x => x.formulation_type_id,
                        principalSchema: "ukps",
                        principalTable: "formulation_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_medicines_product_details_therapeutic_areas_therapeutic_are",
                        column: x => x.therapeutic_area_id,
                        principalSchema: "ukps",
                        principalTable: "therapeutic_areas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_record_statuses",
                schema: "ukps",
                columns: table => new
                {
                    medicines_product_detail_id = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    medicine_status_type_id = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_medicines_record_statuses",
                        x => new { x.medicines_product_detail_id, x.medicine_status_type_id }
                    );
                    table.ForeignKey(
                        name: "fk_medicines_record_statuses_medicine_technology_statuses_medi",
                        column: x => x.medicine_status_type_id,
                        principalSchema: "ukps",
                        principalTable: "medicine_technology_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_medicines_record_statuses_medicines_product_details_medicin",
                        column: x => x.medicines_product_detail_id,
                        principalSchema: "ukps",
                        principalTable: "medicines_product_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_service_impacts",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    existing_nhs_service = table.Column<int>(type: "integer", nullable: true),
                    nhs_service_redesign_details = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    uk_patient_population_range_id = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    uk_patient_population_notes = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    estimated_eligible_patient_population = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    compassionate_access_available = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    compassionate_access_details = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_service_impacts", x => x.id);
                    table.ForeignKey(
                        name: "fk_medicines_service_impacts_uk_patient_population_ranges_uk_p",
                        column: x => x.uk_patient_population_range_id,
                        principalSchema: "ukps",
                        principalTable: "uk_patient_population_range",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "medicines_treatment_details",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    proposed_place_in_therapy = table.Column<string>(type: "text", nullable: false),
                    estimated_duration_of_treatment = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medicines_treatment_details", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "other_clinical_trial_numbers",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    clinical_trial_id = table.Column<int>(type: "integer", nullable: false),
                    other_registry_number = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_other_clinical_trial_numbers", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "qa_review_items",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    qa_review_id = table.Column<int>(type: "integer", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: false),
                    issue_type = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    resolution_status = table.Column<int>(type: "integer", nullable: false),
                    resolved_by = table.Column<int>(type: "integer", nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qa_review_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_qa_review_items_app_user_resolved_by",
                        column: x => x.resolved_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "qa_reviews",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    major_revision_submission_round_no = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    outcome = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    reviewed_by = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qa_reviews", x => x.id);
                    table.ForeignKey(
                        name: "fk_qa_reviews_app_user_reviewed_by",
                        column: x => x.reviewed_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "record_clinical_trials",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    study_name = table.Column<string>(type: "text", nullable: false),
                    clinical_trials_gov_number = table.Column<string>(type: "text", nullable: true),
                    brief_description = table.Column<string>(type: "text", nullable: true),
                    recruiting_in_uk = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_clinical_trials", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "record_event_field_changes",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    record_event_id = table.Column<int>(type: "integer", nullable: false),
                    field_path = table.Column<string>(type: "text", nullable: false),
                    old_value = table.Column<string>(type: "text", nullable: true),
                    new_value = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_event_field_changes", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "record_events",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    record_id = table.Column<int>(type: "integer", nullable: false),
                    revision_id = table.Column<int>(type: "integer", nullable: true),
                    qa_review_id = table.Column<int>(type: "integer", nullable: true),
                    qa_review_item_id = table.Column<int>(type: "integer", nullable: true),
                    event_type = table.Column<int>(type: "integer", nullable: false),
                    performed_by = table.Column<int>(type: "integer", nullable: true),
                    performed_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    payload = table.Column<string>(type: "jsonb", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_events_app_user_performed_by",
                        column: x => x.performed_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_events_qa_review_items_qa_review_item_id",
                        column: x => x.qa_review_item_id,
                        principalSchema: "ukps",
                        principalTable: "qa_review_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_events_qa_reviews_qa_review_id",
                        column: x => x.qa_review_id,
                        principalSchema: "ukps",
                        principalTable: "qa_reviews",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "record_global_submissions",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    global_first_submission_region = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    global_first_submission_notes = table.Column<string>(
                        type: "text",
                        nullable: true
                    ),
                    global_submission_estimated_date_id = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    global_submission_actual_date_id = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_global_submissions", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "record_hta_bodies",
                schema: "ukps",
                columns: table => new
                {
                    record_hta_id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_hta_bodies", x => new { x.record_hta_id, x.label });
                }
            );

            migrationBuilder.CreateTable(
                name: "record_htas",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    hta_body_vaccine = table.Column<string>(type: "text", nullable: true),
                    hta_nice_aligned_pathway = table.Column<int>(type: "integer", nullable: true),
                    hta_additional_details = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_htas", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "record_intl_recognitions",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    irp_reference_regulator_id = table.Column<int>(type: "integer", nullable: true),
                    irp_route_id = table.Column<int>(type: "integer", nullable: true),
                    intl_conditional_approval_anticipated = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    intl_submission_date_id = table.Column<int>(type: "integer", nullable: true),
                    intl_licence_date_id = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_intl_recognitions", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_intl_recognitions_irp_reference_regulators_irp_refer",
                        column: x => x.irp_reference_regulator_id,
                        principalSchema: "ukps",
                        principalTable: "irp_reference_regulator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_intl_recognitions_irp_routes_irp_route_id",
                        column: x => x.irp_route_id,
                        principalSchema: "ukps",
                        principalTable: "irp_route",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "record_mhra_dates",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    uk_submission_date_id = table.Column<int>(type: "integer", nullable: true),
                    uk_licence_date_id = table.Column<int>(type: "integer", nullable: true),
                    uk_launch_date_id = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_mhra_dates", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "record_mhra_procedures",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    mhra_procedure_type_id = table.Column<int>(type: "integer", nullable: true),
                    procedure_details = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_mhra_procedures", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_mhra_procedures_mhra_procedure_types_mhra_procedure_",
                        column: x => x.mhra_procedure_type_id,
                        principalSchema: "ukps",
                        principalTable: "mhra_procedure_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "record_revisions",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    record_id = table.Column<int>(type: "integer", nullable: false),
                    based_on_revision_id = table.Column<int>(type: "integer", nullable: true),
                    revision_no = table.Column<int>(type: "integer", nullable: false),
                    major_version = table.Column<int>(type: "integer", nullable: false),
                    minor_version = table.Column<int>(type: "integer", nullable: false),
                    workflow_status = table.Column<int>(type: "integer", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_by = table.Column<int>(type: "integer", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    submitted_by = table.Column<int>(type: "integer", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_revisions_app_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_revisions_app_user_submitted_by",
                        column: x => x.submitted_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_revisions_app_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_revisions_record_revisions_based_on_revision_id",
                        column: x => x.based_on_revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "records",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    organisation_id = table.Column<int>(type: "integer", nullable: false),
                    record_type = table.Column<int>(type: "integer", nullable: false),
                    record_status = table.Column<int>(type: "integer", nullable: false),
                    published_revision_id = table.Column<int>(type: "integer", nullable: true),
                    current_draft_revision_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: true),
                    reviewed_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_records_app_user_created_by",
                        column: x => x.created_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_records_organisations_organisation_id",
                        column: x => x.organisation_id,
                        principalSchema: "ukps",
                        principalTable: "organisations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_records_record_revisions_current_draft_revision_id",
                        column: x => x.current_draft_revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_records_record_revisions_published_revision_id",
                        column: x => x.published_revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "regulatory_dates",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    date_event = table.Column<int>(type: "integer", nullable: false),
                    date_precision = table.Column<int>(type: "integer", nullable: false),
                    date_value = table.Column<DateOnly>(type: "date", nullable: false),
                    is_confidential = table.Column<bool>(type: "boolean", nullable: false),
                    conditional_approval_anticipated = table.Column<bool>(
                        type: "boolean",
                        nullable: true
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_regulatory_dates", x => x.id);
                    table.ForeignKey(
                        name: "fk_regulatory_dates_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_company_infos",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    is_originator_company = table.Column<int>(type: "integer", nullable: true),
                    originator_company_name = table.Column<string>(type: "text", nullable: true),
                    has_been_acquired = table.Column<int>(type: "integer", nullable: false),
                    previous_owner = table.Column<string>(type: "text", nullable: true),
                    has_grant_funding = table.Column<int>(type: "integer", nullable: false),
                    grant_funding_identifier = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_company_infos", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_company_infos_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_disease_details",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    disease_area_id = table.Column<int>(type: "integer", nullable: true),
                    disease_target = table.Column<string>(type: "text", nullable: false),
                    age_group = table.Column<string>(type: "text", nullable: false),
                    risk_group = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_disease_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_disease_details_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_vaccines_disease_details_vaccine_disease_areas_disease_area",
                        column: x => x.disease_area_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_disease_area",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_populations",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    age_group = table.Column<string>(type: "text", nullable: true),
                    risk_group = table.Column<string>(type: "text", nullable: true),
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
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_product_details",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    record_title = table.Column<string>(type: "text", nullable: false),
                    company_code = table.Column<string>(type: "text", nullable: false),
                    branded_name = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_product_details", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_product_details_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_service_readinesses",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    storage_requirement_id = table.Column<int>(type: "integer", nullable: false),
                    requires_reconstitution = table.Column<int>(type: "integer", nullable: false),
                    additional_service_notes = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_service_readinesses", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_service_readinesses_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_vaccines_service_readinesses_vaccine_storage_requirements_s",
                        column: x => x.storage_requirement_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_storage_requirement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_technologies",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    revision_id = table.Column<int>(type: "integer", nullable: false),
                    vaccine_platform_id = table.Column<int>(type: "integer", nullable: false),
                    vaccine_platform_other = table.Column<string>(type: "text", nullable: true),
                    administration_route_id = table.Column<int>(type: "integer", nullable: false),
                    has_adjuvant = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_technologies", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_technologies_record_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "ukps",
                        principalTable: "record_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_vaccines_technologies_vaccine_administration_routes_adminis",
                        column: x => x.administration_route_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_administration_route",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_vaccines_technologies_vaccine_platforms_vaccine_platform_id",
                        column: x => x.vaccine_platform_id,
                        principalSchema: "ukps",
                        principalTable: "vaccine_platform",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "record_status_histories",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    record_id = table.Column<int>(type: "integer", nullable: false),
                    from_status = table.Column<int>(type: "integer", nullable: true),
                    to_status = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<int>(type: "integer", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_status_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_record_status_histories_app_user_updated_by",
                        column: x => x.updated_by,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_status_histories_records_record_id",
                        column: x => x.record_id,
                        principalSchema: "ukps",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "record_watchlists",
                schema: "ukps",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    record_id = table.Column<int>(type: "integer", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_record_watchlists", x => new { x.user_id, x.record_id });
                    table.ForeignKey(
                        name: "fk_record_watchlists_records_record_id",
                        column: x => x.record_id,
                        principalSchema: "ukps",
                        principalTable: "records",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                    table.ForeignKey(
                        name: "fk_record_watchlists_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "ukps",
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_pathogens",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    vaccines_disease_detail_id = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    pathogen_name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_pathogens", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_pathogens_vaccines_disease_details_vaccines_diseas",
                        column: x => x.vaccines_disease_detail_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_disease_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_company_codes",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    vaccines_product_detail_id = table.Column<int>(
                        type: "integer",
                        nullable: false
                    ),
                    name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_company_codes", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_company_codes_vaccines_product_details_vaccines_pr",
                        column: x => x.vaccines_product_detail_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_product_details",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_adjuvants",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    vaccines_technology_id = table.Column<int>(type: "integer", nullable: false),
                    adjuvant_name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_adjuvants", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_adjuvants_vaccines_technologies_vaccines_technolog",
                        column: x => x.vaccines_technology_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_technologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "vaccines_antigens",
                schema: "ukps",
                columns: table => new
                {
                    id = table
                        .Column<int>(type: "integer", nullable: false)
                        .Annotation(
                            "Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                        ),
                    vaccines_technology_id = table.Column<int>(type: "integer", nullable: false),
                    antigen_name = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vaccines_antigens", x => x.id);
                    table.ForeignKey(
                        name: "fk_vaccines_antigens_vaccines_technologies_vaccines_technology",
                        column: x => x.vaccines_technology_id,
                        principalSchema: "ukps",
                        principalTable: "vaccines_technologies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_app_user_username",
                schema: "ukps",
                table: "app_user",
                column: "username",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_bnf_chapters_parent_id",
                schema: "ukps",
                table: "bnf_chapters",
                column: "parent_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_email_audits_template_id",
                schema: "ukps",
                table: "email_audits",
                column: "template_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_active_substance_product_detail_id",
                schema: "ukps",
                table: "medicines_active_substances",
                column: "medicines_product_detail_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_budget_impact_revision_id",
                schema: "ukps",
                table: "medicines_budget_impacts",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_company_info_revision_id",
                schema: "ukps",
                table: "medicines_company_infos",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_detail_revision_id",
                schema: "ukps",
                table: "medicines_details",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eams_pim_revision_id",
                schema: "ukps",
                table: "medicines_eams_pims",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eams_pims_eams_opinion_date_id",
                schema: "ukps",
                table: "medicines_eams_pims",
                column: "eams_opinion_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eams_pims_eams_submission_date_id",
                schema: "ukps",
                table: "medicines_eams_pims",
                column: "eams_submission_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eu_status_revision_id",
                schema: "ukps",
                table: "medicines_eu_statuses",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eu_statuses_atmp_classification_id",
                schema: "ukps",
                table: "medicines_eu_statuses",
                column: "atmp_classification_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_eu_statuses_eu_orphan_granted_date_id",
                schema: "ukps",
                table: "medicines_eu_statuses",
                column: "eu_orphan_granted_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_laboratory_testing_revision_id",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_laboratory_testings_genomic_sample_type_id",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                column: "genomic_sample_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_laboratory_testings_patient_pathway_point_id",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                column: "patient_pathway_point_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_pas_regions_pas_region_id",
                schema: "ukps",
                table: "medicines_pas_regions",
                column: "pas_region_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_patient_identification_revision_id",
                schema: "ukps",
                table: "medicines_patient_identifications",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_detail_revision_id",
                schema: "ukps",
                table: "medicines_product_details",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_details_bnf_chapter_id",
                schema: "ukps",
                table: "medicines_product_details",
                column: "bnf_chapter_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_details_formulation_type_id",
                schema: "ukps",
                table: "medicines_product_details",
                column: "formulation_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_product_details_therapeutic_area_id",
                schema: "ukps",
                table: "medicines_product_details",
                column: "therapeutic_area_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_record_statuses_medicine_status_type_id",
                schema: "ukps",
                table: "medicines_record_statuses",
                column: "medicine_status_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_service_impact_revision_id",
                schema: "ukps",
                table: "medicines_service_impacts",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_service_impacts_uk_patient_population_range_id",
                schema: "ukps",
                table: "medicines_service_impacts",
                column: "uk_patient_population_range_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_medicines_treatment_detail_revision_id",
                schema: "ukps",
                table: "medicines_treatment_details",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_organisation_audit_organisation_id",
                schema: "ukps",
                table: "organisation_audits",
                column: "organisation_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_organisation_audits_updated_by",
                schema: "ukps",
                table: "organisation_audits",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_other_clinical_trial_number_clinical_trial_id",
                schema: "ukps",
                table: "other_clinical_trial_numbers",
                column: "clinical_trial_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_qa_review_item_qa_review_id",
                schema: "ukps",
                table: "qa_review_items",
                column: "qa_review_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_qa_review_items_resolved_by",
                schema: "ukps",
                table: "qa_review_items",
                column: "resolved_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_qa_review_revision_id",
                schema: "ukps",
                table: "qa_reviews",
                column: "revision_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_qa_reviews_reviewed_by",
                schema: "ukps",
                table: "qa_reviews",
                column: "reviewed_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_clinical_trial_revision_id",
                schema: "ukps",
                table: "record_clinical_trials",
                column: "revision_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_event_field_change_record_event_id",
                schema: "ukps",
                table: "record_event_field_changes",
                column: "record_event_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_event_record_id_event_type",
                schema: "ukps",
                table: "record_events",
                columns: new[] { "record_id", "event_type" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_event_revision_id",
                schema: "ukps",
                table: "record_events",
                column: "revision_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_events_performed_by",
                schema: "ukps",
                table: "record_events",
                column: "performed_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_events_qa_review_id",
                schema: "ukps",
                table: "record_events",
                column: "qa_review_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_events_qa_review_item_id",
                schema: "ukps",
                table: "record_events",
                column: "qa_review_item_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_global_submission_revision_id",
                schema: "ukps",
                table: "record_global_submissions",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_global_submissions_global_submission_actual_date_id",
                schema: "ukps",
                table: "record_global_submissions",
                column: "global_submission_actual_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_global_submissions_global_submission_estimated_date_",
                schema: "ukps",
                table: "record_global_submissions",
                column: "global_submission_estimated_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_hta_revision_id",
                schema: "ukps",
                table: "record_htas",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognition_revision_id",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognitions_intl_licence_date_id",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "intl_licence_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognitions_intl_submission_date_id",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "intl_submission_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognitions_irp_reference_regulator_id",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "irp_reference_regulator_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_intl_recognitions_irp_route_id",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "irp_route_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_date_revision_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_dates_uk_launch_date_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "uk_launch_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_dates_uk_licence_date_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "uk_licence_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_dates_uk_submission_date_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "uk_submission_date_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_procedure_revision_id",
                schema: "ukps",
                table: "record_mhra_procedures",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_mhra_procedures_mhra_procedure_type_id",
                schema: "ukps",
                table: "record_mhra_procedures",
                column: "mhra_procedure_type_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_record_id_major_minor",
                schema: "ukps",
                table: "record_revisions",
                columns: new[] { "record_id", "major_version", "minor_version" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_revision_record_id_revision_no",
                schema: "ukps",
                table: "record_revisions",
                columns: new[] { "record_id", "revision_no" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_revisions_based_on_revision_id",
                schema: "ukps",
                table: "record_revisions",
                column: "based_on_revision_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_revisions_created_by",
                schema: "ukps",
                table: "record_revisions",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_revisions_submitted_by",
                schema: "ukps",
                table: "record_revisions",
                column: "submitted_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_revisions_updated_by",
                schema: "ukps",
                table: "record_revisions",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_status_histories_updated_by",
                schema: "ukps",
                table: "record_status_histories",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_status_history_record_id",
                schema: "ukps",
                table: "record_status_histories",
                column: "record_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_watchlists_record_id",
                schema: "ukps",
                table: "record_watchlists",
                column: "record_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_organisation_id",
                schema: "ukps",
                table: "records",
                column: "organisation_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_type_status_reviewed_at",
                schema: "ukps",
                table: "records",
                columns: new[] { "record_type", "record_status", "reviewed_at" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_records_created_by",
                schema: "ukps",
                table: "records",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_records_current_draft_revision_id",
                schema: "ukps",
                table: "records",
                column: "current_draft_revision_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_records_published_revision_id",
                schema: "ukps",
                table: "records",
                column: "published_revision_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_regulatory_date_revision_event_precision",
                schema: "ukps",
                table: "regulatory_dates",
                columns: new[] { "revision_id", "date_event", "date_precision" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_report_audits_report_preset_id",
                schema: "ukps",
                table: "report_audits",
                column: "report_preset_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_report_audits_user_id",
                schema: "ukps",
                table: "report_audits",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_report_presets_created_by",
                schema: "ukps",
                table: "report_presets",
                column: "created_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_terms_acceptances_organisation_id",
                schema: "ukps",
                table: "terms_acceptances",
                column: "organisation_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_therapeutic_areas_parent_id",
                schema: "ukps",
                table: "therapeutic_areas",
                column: "parent_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_audit_user_id",
                schema: "ukps",
                table: "user_audits",
                column: "user_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_audits_updated_by",
                schema: "ukps",
                table: "user_audits",
                column: "updated_by"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_org_membership_organisation_id",
                schema: "ukps",
                table: "user_org_memberships",
                column: "organisation_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_user_org_membership_user_org_entity",
                schema: "ukps",
                table: "user_org_memberships",
                columns: new[] { "user_id", "organisation_id", "allowed_pharmaceutical_entity" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_adjuvant_technology_id",
                schema: "ukps",
                table: "vaccines_adjuvants",
                column: "vaccines_technology_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_antigen_technology_id",
                schema: "ukps",
                table: "vaccines_antigens",
                column: "vaccines_technology_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_company_code_product_detail_id",
                schema: "ukps",
                table: "vaccines_company_codes",
                column: "vaccines_product_detail_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_company_info_revision_id",
                schema: "ukps",
                table: "vaccines_company_infos",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_disease_detail_revision_id",
                schema: "ukps",
                table: "vaccines_disease_details",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_disease_details_disease_area_id",
                schema: "ukps",
                table: "vaccines_disease_details",
                column: "disease_area_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_pathogen_disease_detail_id",
                schema: "ukps",
                table: "vaccines_pathogens",
                column: "vaccines_disease_detail_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_population_revision_id",
                schema: "ukps",
                table: "vaccines_populations",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_product_detail_revision_id",
                schema: "ukps",
                table: "vaccines_product_details",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_service_readiness_revision_id",
                schema: "ukps",
                table: "vaccines_service_readinesses",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_service_readinesses_storage_requirement_id",
                schema: "ukps",
                table: "vaccines_service_readinesses",
                column: "storage_requirement_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_technologies_administration_route_id",
                schema: "ukps",
                table: "vaccines_technologies",
                column: "administration_route_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_technologies_vaccine_platform_id",
                schema: "ukps",
                table: "vaccines_technologies",
                column: "vaccine_platform_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_vaccines_technology_revision_id",
                schema: "ukps",
                table: "vaccines_technologies",
                column: "revision_id",
                unique: true
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_active_substances_medicines_product_details_medic",
                schema: "ukps",
                table: "medicines_active_substances",
                column: "medicines_product_detail_id",
                principalSchema: "ukps",
                principalTable: "medicines_product_details",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_budget_impacts_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_budget_impacts",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_company_infos_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_company_infos",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_details_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_details",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eams_pims_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_eams_pims",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eams_pims_regulatory_dates_eams_opinion_date_id",
                schema: "ukps",
                table: "medicines_eams_pims",
                column: "eams_opinion_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eams_pims_regulatory_dates_eams_submission_date_id",
                schema: "ukps",
                table: "medicines_eams_pims",
                column: "eams_submission_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eu_statuses_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_eu_statuses",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_eu_statuses_regulatory_dates_eu_orphan_granted_da",
                schema: "ukps",
                table: "medicines_eu_statuses",
                column: "eu_orphan_granted_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_laboratory_testings_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_laboratory_testings",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_patient_identifications_record_revisions_revision",
                schema: "ukps",
                table: "medicines_patient_identifications",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_product_details_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_product_details",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_service_impacts_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_service_impacts",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_medicines_treatment_details_record_revisions_revision_id",
                schema: "ukps",
                table: "medicines_treatment_details",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_other_clinical_trial_numbers_record_clinical_trials_clinica",
                schema: "ukps",
                table: "other_clinical_trial_numbers",
                column: "clinical_trial_id",
                principalSchema: "ukps",
                principalTable: "record_clinical_trials",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_qa_review_items_qa_reviews_qa_review_id",
                schema: "ukps",
                table: "qa_review_items",
                column: "qa_review_id",
                principalSchema: "ukps",
                principalTable: "qa_reviews",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_qa_reviews_record_revisions_revision_id",
                schema: "ukps",
                table: "qa_reviews",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_clinical_trials_record_revisions_revision_id",
                schema: "ukps",
                table: "record_clinical_trials",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_event_field_changes_record_events_record_event_id",
                schema: "ukps",
                table: "record_event_field_changes",
                column: "record_event_id",
                principalSchema: "ukps",
                principalTable: "record_events",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_events_record_revisions_revision_id",
                schema: "ukps",
                table: "record_events",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_events_records_record_id",
                schema: "ukps",
                table: "record_events",
                column: "record_id",
                principalSchema: "ukps",
                principalTable: "records",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_global_submissions_record_revisions_revision_id",
                schema: "ukps",
                table: "record_global_submissions",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_global_submissions_regulatory_dates_global_submissio",
                schema: "ukps",
                table: "record_global_submissions",
                column: "global_submission_actual_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_global_submissions_regulatory_dates_global_submissio1",
                schema: "ukps",
                table: "record_global_submissions",
                column: "global_submission_estimated_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_hta_bodies_record_htas_record_hta_id",
                schema: "ukps",
                table: "record_hta_bodies",
                column: "record_hta_id",
                principalSchema: "ukps",
                principalTable: "record_htas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_htas_record_revisions_revision_id",
                schema: "ukps",
                table: "record_htas",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_intl_recognitions_record_revisions_revision_id",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_intl_recognitions_regulatory_dates_intl_licence_date",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "intl_licence_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_intl_recognitions_regulatory_dates_intl_submission_d",
                schema: "ukps",
                table: "record_intl_recognitions",
                column: "intl_submission_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_mhra_dates_record_revisions_revision_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_mhra_dates_regulatory_dates_uk_launch_date_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "uk_launch_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_mhra_dates_regulatory_dates_uk_licence_date_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "uk_licence_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_mhra_dates_regulatory_dates_uk_submission_date_id",
                schema: "ukps",
                table: "record_mhra_dates",
                column: "uk_submission_date_id",
                principalSchema: "ukps",
                principalTable: "regulatory_dates",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_mhra_procedures_record_revisions_revision_id",
                schema: "ukps",
                table: "record_mhra_procedures",
                column: "revision_id",
                principalSchema: "ukps",
                principalTable: "record_revisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_record_revisions_records_record_id",
                schema: "ukps",
                table: "record_revisions",
                column: "record_id",
                principalSchema: "ukps",
                principalTable: "records",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_records_record_revisions_current_draft_revision_id",
                schema: "ukps",
                table: "records"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_records_record_revisions_published_revision_id",
                schema: "ukps",
                table: "records"
            );

            migrationBuilder.DropTable(name: "email_audits", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_active_substances", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_company_infos", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_details", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_eams_pims", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_eu_statuses", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_laboratory_testings", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_pas_regions", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_patient_identifications", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_record_statuses", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_service_impacts", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_treatment_details", schema: "ukps");

            migrationBuilder.DropTable(name: "organisation_audits", schema: "ukps");

            migrationBuilder.DropTable(name: "other_clinical_trial_numbers", schema: "ukps");

            migrationBuilder.DropTable(name: "record_event_field_changes", schema: "ukps");

            migrationBuilder.DropTable(name: "record_global_submissions", schema: "ukps");

            migrationBuilder.DropTable(name: "record_hta_bodies", schema: "ukps");

            migrationBuilder.DropTable(name: "record_intl_recognitions", schema: "ukps");

            migrationBuilder.DropTable(name: "record_mhra_dates", schema: "ukps");

            migrationBuilder.DropTable(name: "record_mhra_procedures", schema: "ukps");

            migrationBuilder.DropTable(name: "record_status_histories", schema: "ukps");

            migrationBuilder.DropTable(name: "record_watchlists", schema: "ukps");

            migrationBuilder.DropTable(name: "report_audits", schema: "ukps");

            migrationBuilder.DropTable(name: "terms_acceptances", schema: "ukps");

            migrationBuilder.DropTable(name: "user_audits", schema: "ukps");

            migrationBuilder.DropTable(name: "user_org_memberships", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_adjuvants", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_antigens", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_company_codes", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_company_infos", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_pathogens", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_populations", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_service_readinesses", schema: "ukps");

            migrationBuilder.DropTable(name: "email_templates", schema: "ukps");

            migrationBuilder.DropTable(name: "atmp_classification", schema: "ukps");

            migrationBuilder.DropTable(name: "genomic_sample_type", schema: "ukps");

            migrationBuilder.DropTable(name: "patient_pathway_point", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_budget_impacts", schema: "ukps");

            migrationBuilder.DropTable(name: "pas_region", schema: "ukps");

            migrationBuilder.DropTable(name: "medicine_technology_status", schema: "ukps");

            migrationBuilder.DropTable(name: "medicines_product_details", schema: "ukps");

            migrationBuilder.DropTable(name: "uk_patient_population_range", schema: "ukps");

            migrationBuilder.DropTable(name: "record_clinical_trials", schema: "ukps");

            migrationBuilder.DropTable(name: "record_events", schema: "ukps");

            migrationBuilder.DropTable(name: "record_htas", schema: "ukps");

            migrationBuilder.DropTable(name: "irp_reference_regulator", schema: "ukps");

            migrationBuilder.DropTable(name: "irp_route", schema: "ukps");

            migrationBuilder.DropTable(name: "regulatory_dates", schema: "ukps");

            migrationBuilder.DropTable(name: "mhra_procedure_type", schema: "ukps");

            migrationBuilder.DropTable(name: "report_presets", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_technologies", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_product_details", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccines_disease_details", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccine_storage_requirement", schema: "ukps");

            migrationBuilder.DropTable(name: "bnf_chapters", schema: "ukps");

            migrationBuilder.DropTable(name: "formulation_type", schema: "ukps");

            migrationBuilder.DropTable(name: "therapeutic_areas", schema: "ukps");

            migrationBuilder.DropTable(name: "qa_review_items", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccine_administration_route", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccine_platform", schema: "ukps");

            migrationBuilder.DropTable(name: "vaccine_disease_area", schema: "ukps");

            migrationBuilder.DropTable(name: "qa_reviews", schema: "ukps");

            migrationBuilder.DropTable(name: "record_revisions", schema: "ukps");

            migrationBuilder.DropTable(name: "records", schema: "ukps");

            migrationBuilder.DropTable(name: "app_user", schema: "ukps");

            migrationBuilder.DropTable(name: "organisations", schema: "ukps");
        }
    }
}
