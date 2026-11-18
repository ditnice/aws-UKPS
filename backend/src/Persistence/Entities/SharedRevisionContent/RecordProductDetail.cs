using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.SharedRevisionContent;

/// <summary>Names and identifiers for the product.</summary>
/// <remarks>
/// Record creation asks the same questions and inserts the row, so its rules must be at least as
/// strict as these, and its field audit paths are these question IDs.
/// </remarks>
[FormEntity(RowSource = FormRowSource.RecordCreation)]
internal sealed class RecordProductDetail
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    /// <summary>
    /// Internal code or working name e.g. ABC-123, mRNA-1273, BNT162b2.
    /// Primary identifier at pipeline stage.
    /// </summary>
    [FormPage(MedicineFormLayout.NamesAndIdentifiers)]
    [FormField(
        CreationPageFieldPaths.CompanyCode,
        "Company code",
        FormControl.Text,
        Hint = "Enter the primary internal code or working name your company uses for this product seeking market access.",
        Order = 10
    )]
    [FormRequired("Enter company code")]
    public required string CompanyCode { get; set; }

    [FormPage(MedicineFormLayout.NamesAndIdentifiers)]
    [FormField(
        CreationPageFieldPaths.BrandedName,
        "Branded name (optional)",
        FormControl.Text,
        Hint = "Enter the name this medicine is sold under. For example, Humira, Lipitor.",
        Order = 30
    )]
    public string? BrandedName { get; set; }

    /// <summary>
    /// Short human-readable label to identify this record on the homepage.
    /// e.g. Chronic hepatitis C in adults, RSV — adults 60+.
    /// </summary>
    [FormPage(MedicineFormLayout.NamesAndIdentifiers)]
    [FormField(
        CreationPageFieldPaths.RecordTitle,
        "Record title",
        FormControl.Textarea,
        Hint = "Enter a title to help you to identify this record. This information is not shown to horizon scanning users. You can enter up to 100 characters.",
        Order = 50
    )]
    [FormRequired("Enter a record title")]
    [FormMaxLength(100, "Record title must be between 1 and 100 characters")]
    public required string RecordTitle { get; set; }

    // Navigation
    public RecordWorkflow.RecordRevision? Revision { get; set; }

    /// <summary>
    /// Generic names (medicines only) and other names, codes, or synonyms.
    /// </summary>
    public ICollection<RecordNameAndIdentifier> NamesAndIdentifiers { get; set; } = [];

    /// <summary>The other names, codes or synonyms, in display order. Not mapped.</summary>
    [FormPage(MedicineFormLayout.NamesAndIdentifiers)]
    [FormField(
        CreationPageFieldPaths.OtherIdentifiers,
        "Other names and identifiers (optional)",
        FormControl.TextList,
        Hint = "Enter any other development names, synonyms, or identifiers that you want to associate with this product.",
        AddAnotherLabel = "Add another identifier",
        ItemLabel = "Identifier",
        Order = 20
    )]
    [FormListSource(nameof(NamesAndIdentifiers), NameAndIdentifierType.OtherIdentifier)]
    public IEnumerable<string> OtherIdentifiers => Names(NameAndIdentifierType.OtherIdentifier);

    /// <summary>The generic names, in display order. Not mapped.</summary>
    [FormPage(MedicineFormLayout.NamesAndIdentifiers)]
    [FormField(
        CreationPageFieldPaths.GenericNames,
        "Active substance",
        FormControl.TextList,
        Hint = "Enter the standard, non-proprietary name for the active substances. For example, adalimumab, atorvastatin.",
        AddAnotherLabel = "Add another active substance",
        ItemLabel = "Generic name",
        Order = 40
    )]
    [FormRequired("Enter generic name")]
    [FormDistinct("Generic names must be distinct")]
    [FormListSource(nameof(NamesAndIdentifiers), NameAndIdentifierType.GenericName)]
    public IEnumerable<string> GenericNames => Names(NameAndIdentifierType.GenericName);

    private IEnumerable<string> Names(NameAndIdentifierType type) =>
        NamesAndIdentifiers
            .Where(x => x.NameType == type)
            .OrderBy(static x => x.DisplayOrder)
            .ThenBy(static x => x.Id)
            .Select(static x => x.Name);
}
