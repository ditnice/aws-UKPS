namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

/// <summary>
/// The <see cref="RecordFieldAudit.FieldPath"/> of each field answered on the page that creates a
/// record, shared by record creation and the synthetic data seeder so the two cannot drift apart.
/// They are also the question IDs of the names and identifiers form page, so creation and later
/// edits audit each field under one path.
/// </summary>
internal static class CreationPageFieldPaths
{
    public const string CompanyCode = "record_product_detail.company_code";
    public const string BrandedName = "record_product_detail.branded_name";
    public const string RecordTitle = "record_product_detail.record_title";
    public const string GenericNames = "record_product_detail.generic_names";
    public const string OtherIdentifiers = "record_product_detail.other_identifiers";
}
