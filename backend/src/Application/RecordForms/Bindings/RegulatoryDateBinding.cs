using System.Linq.Expressions;
using System.Reflection;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// Stores a date answer as a <see cref="RegulatoryDate"/> row that the content row's foreign key
/// points to. The row is created on the first answer, updated in place, and deleted when the
/// answer is cleared. The navigation must be loaded with the content row. The answer is audited
/// as the row's contents. The control decides how the answer is read, as in
/// <see cref="DateAnswer.TryParse"/>.
/// </summary>
internal sealed class RegulatoryDateBinding(
    PropertyInfo foreignKey,
    PropertyInfo navigation,
    PropertyInfo revisionId,
    DateEventType dateEvent,
    FormControl control
) : IFormBinding
{
    public AnswerShape Shape => AnswerShape.Single;

    public string Description =>
        $"regulatory-date:{foreignKey.DeclaringType!.Name}.{foreignKey.Name}:{dateEvent}";

    public FormAnswer Read(object? row) =>
        FormAnswer.Single(
            row is not null && navigation.GetValue(row) is RegulatoryDate date
                ? DateAnswer.Format(date.DatePrecision, date.DateValue)
                : null
        );

    public void Write(AppDbContext dbContext, object row, FormAnswer answer)
    {
        RegulatoryDate? date = navigation.GetValue(row) as RegulatoryDate;
        if (answer.Value is null)
        {
            if (date is not null)
            {
                navigation.SetValue(row, null);
                foreignKey.SetValue(row, null);
                dbContext.Remove(date);
            }

            return;
        }

        (DatePrecision precision, DateOnly value) = Parse(answer.Value);
        if (date is null)
        {
            date = new RegulatoryDate
            {
                RevisionId = (int)revisionId.GetValue(row)!,
                DateEvent = dateEvent,
            };
            navigation.SetValue(row, date);
        }

        date.DatePrecision = precision;
        date.DateValue = value;
        date.IsConfidential = IsConfidential(precision);
    }

    public string? AuditText(FormAnswer answer)
    {
        if (answer.Value is null)
        {
            return null;
        }

        (DatePrecision precision, DateOnly value) = Parse(answer.Value);
        return FieldAuditJson.RegulatoryDate(precision, value, IsConfidential(precision));
    }

    public Expression HasAnswerExpression(ParameterExpression row) =>
        Expression.Property(Expression.Property(row, foreignKey), nameof(Nullable<int>.HasValue));

    /// <summary>Estimated dates are commercial in confidence; actual dates are not.</summary>
    private static bool IsConfidential(DatePrecision precision) =>
        precision is DatePrecision.EstimatedQuarter or DatePrecision.EstimatedMonth;

    private (DatePrecision Precision, DateOnly Value) Parse(string answer) =>
        DateAnswer.TryParse(control, answer, out DatePrecision precision, out DateOnly value)
            ? (precision, value)
            : throw new InvalidOperationException(
                $"'{answer}' is not a valid date answer; answers are validated before they are used."
            );
}
