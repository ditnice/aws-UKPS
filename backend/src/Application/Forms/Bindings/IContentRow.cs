namespace UKPS.Api.Application.Forms.Bindings;

/// <summary>
/// Describes how to find a revision's single row in a top-level content table.
/// </summary>
internal interface IContentRow
{
    Type EntityType { get; }
}
