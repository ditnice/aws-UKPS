namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>The projection a reference data provider maps each row to.</summary>
internal sealed record ReferenceOptionRow(int Id, string Label, bool IsArchived, int? ParentId);
