namespace UKPS.Api.Application.Forms.Options;

/// <summary>A row of reference data projected for use as an option.</summary>
internal sealed record ReferenceOptionRow(int Id, string Label, bool IsArchived);
