using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UKPS.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordFormSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "ukps",
                table: "record_revisions",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.InsertData(
                schema: "ukps",
                table: "bnf_chapters",
                columns: new[] { "id", "code", "display_order", "is_archived", "label", "parent_id" },
                values: new object[,]
                {
                    { 1, "1", 1, false, "Gastro-intestinal system", null },
                    { 2, "2", 2, false, "Cardiovascular system", null },
                    { 3, "3", 3, false, "Respiratory system", null },
                    { 4, "4", 4, false, "Central nervous system", null },
                    { 5, "5", 5, false, "Infections", null },
                    { 6, "6", 6, false, "Endocrine system", null },
                    { 7, "7", 7, false, "Obstetrics, gynaecology, and urinary-tract disorders", null },
                    { 8, "8", 8, false, "Malignant disease and immunosuppression", null },
                    { 9, "9", 9, false, "Nutrition and blood", null },
                    { 10, "10", 10, false, "Musculoskeletal and joint diseases", null },
                    { 11, "11", 11, false, "Eye", null },
                    { 12, "12", 12, false, "Ear, nose, and oropharynx", null },
                    { 13, "13", 13, false, "Skin", null },
                    { 14, "14", 14, false, "Immunological products and vaccines", null },
                    { 15, "15", 15, false, "Anaesthesia", null }
                });

            migrationBuilder.InsertData(
                schema: "ukps",
                table: "therapeutic_areas",
                columns: new[] { "id", "display_order", "is_archived", "label", "parent_id" },
                values: new object[,]
                {
                    { 1, 1, false, "1: Addiction", null },
                    { 2, 2, false, "2: Allergy", null },
                    { 3, 3, false, "3: Analgesia, Anesthesiology, or Anti-inflammatory", null },
                    { 4, 4, false, "4: Cardiovascular Disease", null },
                    { 5, 5, false, "5: Dermatology", null },
                    { 6, 6, false, "6: Endocrinology, Metabolism, or Bone", null },
                    { 7, 7, false, "7: Gastroenterology", null },
                    { 8, 8, false, "8: Hematology/Coagulation", null },
                    { 9, 9, false, "9: Immunomodulators", null },
                    { 10, 10, false, "10: Viral infectious disease", null },
                    { 11, 11, false, "11: Non-viral infectious Disease", null },
                    { 12, 12, false, "12: Medical Imaging", null },
                    { 13, 13, false, "13: Neurology", null },
                    { 14, 14, false, "14: Oncology", null },
                    { 15, 15, false, "15: Ophthalmology", null },
                    { 16, 16, false, "16: Psychiatry", null },
                    { 17, 17, false, "17: Pulmonary", null },
                    { 18, 18, false, "18: Renal disease", null },
                    { 19, 19, false, "19: Rheumatology", null },
                    { 20, 20, false, "20: Urologic", null }
                });

            // Rows were inserted with explicit IDs, which does not advance the identity
            // sequences. Move them past the seeded rows so new rows don't collide.
            migrationBuilder.Sql(
                "SELECT setval(pg_get_serial_sequence('ukps.bnf_chapters', 'id'), (SELECT MAX(id) FROM ukps.bnf_chapters));"
            );
            migrationBuilder.Sql(
                "SELECT setval(pg_get_serial_sequence('ukps.therapeutic_areas', 'id'), (SELECT MAX(id) FROM ukps.therapeutic_areas));"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "bnf_chapters",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "ukps",
                table: "therapeutic_areas",
                keyColumn: "id",
                keyValue: 20);

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "ukps",
                table: "record_revisions");
        }
    }
}
