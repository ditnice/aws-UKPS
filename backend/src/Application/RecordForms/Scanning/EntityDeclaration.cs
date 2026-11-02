using Microsoft.EntityFrameworkCore.Metadata;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>A content entity with <see cref="FormSectionAttribute"/>, as read by the scanner.</summary>
internal sealed record EntityDeclaration(
    Type ClrType,
    IEntityType EntityType,
    IReadOnlyList<FormSectionAttribute> Sections,
    IReadOnlyList<QuestionDeclaration> Questions
);
