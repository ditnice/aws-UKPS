using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>
/// Where a page sits: its form's record type, and its section and task in that form's layout.
/// </summary>
internal sealed record PagePlacement(
    RecordType RecordType,
    FormLayoutSection Section,
    FormLayoutTask Task,
    FormLayoutPage Page
);
