namespace UKPS.Api.Persistence.Entities.RecordWorkflow;

/// <summary>
/// The <see cref="RecordFieldAudit.FieldPath"/> of each field answered on the page that creates a
/// record, shared by record creation and the synthetic data seeder so the two cannot drift apart.
/// </summary>
internal static class CreationPageFieldPaths
{
    public const string CompanyCode = "record_product_detail.company_code";
    public const string BrandedName = "record_product_detail.branded_name";
    public const string RecordTitle = "record_product_detail.record_title";
    public const string NamesAndIdentifiers = "record_product_detail.names_and_identifiers";
}
