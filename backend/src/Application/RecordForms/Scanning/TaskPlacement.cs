using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Where a task sits: its form's record type and its section in that form's layout.</summary>
internal sealed record TaskPlacement(
    RecordType RecordType,
    FormLayoutSection Section,
    FormLayoutTask Task
);
