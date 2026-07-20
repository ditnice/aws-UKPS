using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedNamespacesForThEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organisation_audits_users_updated_by",
                schema: "ukps",
                table: "organisation_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_qa_review_items_app_user_resolved_by",
                schema: "ukps",
                table: "qa_review_items");

            migrationBuilder.DropForeignKey(
                name: "fk_qa_reviews_app_user_reviewed_by",
                schema: "ukps",
                table: "qa_reviews");

            migrationBuilder.AddForeignKey(
                name: "fk_organisation_audits_app_user_updated_by",
                schema: "ukps",
                table: "organisation_audits",
                column: "updated_by",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_qa_review_items_users_resolved_by",
                schema: "ukps",
                table: "qa_review_items",
                column: "resolved_by",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_qa_reviews_users_reviewed_by",
                schema: "ukps",
                table: "qa_reviews",
                column: "reviewed_by",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organisation_audits_app_user_updated_by",
                schema: "ukps",
                table: "organisation_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_qa_review_items_users_resolved_by",
                schema: "ukps",
                table: "qa_review_items");

            migrationBuilder.DropForeignKey(
                name: "fk_qa_reviews_users_reviewed_by",
                schema: "ukps",
                table: "qa_reviews");

            migrationBuilder.AddForeignKey(
                name: "fk_organisation_audits_users_updated_by",
                schema: "ukps",
                table: "organisation_audits",
                column: "updated_by",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_qa_review_items_app_user_resolved_by",
                schema: "ukps",
                table: "qa_review_items",
                column: "resolved_by",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_qa_reviews_app_user_reviewed_by",
                schema: "ukps",
                table: "qa_reviews",
                column: "reviewed_by",
                principalSchema: "ukps",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
