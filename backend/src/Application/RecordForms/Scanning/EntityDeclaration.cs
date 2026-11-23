using Microsoft.EntityFrameworkCore.Metadata;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>A content entity with <see cref="FormEntityAttribute"/>, as read by the scanner.</summary>
internal sealed record EntityDeclaration(
    Type ClrType,
    IEntityType EntityType,
    FormRowSource RowSource,
    IReadOnlyList<QuestionDeclaration> Questions,
    IReadOnlyList<FormPageShowWhenAttribute> PageConditions
);
