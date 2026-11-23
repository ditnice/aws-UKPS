using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeNameAndIdentifierDisplayOrderNullsNotDistinct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_record_name_and_identifier_product_detail_type_display_order",
                schema: "ukps",
                table: "record_names_and_identifiers");

            migrationBuilder.CreateIndex(
                name: "ix_record_name_and_identifier_product_detail_type_display_order",
                schema: "ukps",
                table: "record_names_and_identifiers",
                columns: new[] { "record_product_detail_id", "name_type", "display_order" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_record_name_and_identifier_product_detail_type_display_order",
                schema: "ukps",
                table: "record_names_and_identifiers");

            migrationBuilder.CreateIndex(
                name: "ix_record_name_and_identifier_product_detail_type_display_order",
                schema: "ukps",
                table: "record_names_and_identifiers",
                columns: new[] { "record_product_detail_id", "name_type", "display_order" },
                unique: true);
        }
    }
}
