namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// The question applies only when another question on the same page is answered with one of
/// <see cref="Values"/>. While it does not apply it is hidden, not validated, and its saved
/// answer is cleared.
/// </summary>
/// <param name="questionId">
/// The ID of the trigger question. It must be on the same page with a lower
/// <see cref="FormFieldAttribute.Order"/>, and be a <see cref="FormControl.Radio"/>,
/// <see cref="FormControl.Select"/> or <see cref="FormControl.Checkbox"/> on an enum or flags
/// enum property, or a <see cref="FormControl.PrecisionDate"/>.
/// </param>
/// <param name="values">
/// Members of the trigger's enum, for example <c>YesNoUnknown.Yes</c>, or
/// <see cref="Enums.DatePrecision"/> members for a date. The condition is met when the trigger's
/// answer includes any of them. At least one, with no duplicates.
/// </param>
/// <remarks>
/// The question must not be bound to a non-nullable column, because its answer is cleared when it
/// does not apply.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
internal sealed class FormShowWhenAttribute(string questionId, params object[] values) : Attribute
{
    /// <summary>The ID of the trigger question.</summary>
    public string QuestionId { get; } = questionId;

    /// <summary>The trigger answers that make the question apply.</summary>
    public IReadOnlyList<object> Values { get; } = values;
}
