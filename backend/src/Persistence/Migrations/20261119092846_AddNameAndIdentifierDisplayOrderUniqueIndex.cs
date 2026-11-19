using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNameAndIdentifierDisplayOrderUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_record_name_and_identifier_product_detail_id",
                schema: "ukps",
                table: "record_names_and_identifiers");

            // Number each list's rows from 1 in the order the form reads them (display order, with
            // none last, then ID), so no existing rows break the unique index.
            migrationBuilder.Sql(
                """
                UPDATE ukps.record_names_and_identifiers AS name
                SET display_order = ordered.position
                FROM (
                    SELECT id, ROW_NUMBER() OVER (
                        PARTITION BY record_product_detail_id, name_type
                        ORDER BY display_order NULLS LAST, id
                    )::integer AS position
                    FROM ukps.record_names_and_identifiers
                ) AS ordered
                WHERE name.id = ordered.id
                    AND name.display_order IS DISTINCT FROM ordered.position;
                """
            );

            migrationBuilder.CreateIndex(
                name: "ix_record_name_and_identifier_product_detail_type_display_order",
                schema: "ukps",
                table: "record_names_and_identifiers",
                columns: new[] { "record_product_detail_id", "name_type", "display_order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_record_name_and_identifier_product_detail_type_display_order",
                schema: "ukps",
                table: "record_names_and_identifiers");

            migrationBuilder.CreateIndex(
                name: "ix_record_name_and_identifier_product_detail_id",
                schema: "ukps",
                table: "record_names_and_identifiers",
                column: "record_product_detail_id");
        }
    }
}
