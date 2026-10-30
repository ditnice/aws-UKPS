using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// Reads and writes one question's answer on a content row. The only form code that touches EF
/// entities.
/// </summary>
internal interface IFormBinding
{
    AnswerShape Shape { get; }

    /// <summary>A stable description of where the answer is stored, for the form fingerprint.</summary>
    string Description { get; }

    /// <summary>Reads the answer from the row; a missing row reads as unanswered.</summary>
    FormAnswer Read(object? row);

    /// <summary>Writes a validated answer to the row.</summary>
    void Write(AppDbContext dbContext, object row, FormAnswer answer);
}
