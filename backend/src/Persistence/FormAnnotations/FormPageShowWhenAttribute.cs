namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// A page of this entity applies only when a question on an earlier page is answered with one of
/// <see cref="Values"/>. While it does not apply it is skipped by Back and Save and continue, and
/// every answer on it is cleared.
/// </summary>
/// <param name="pageId">
/// The page's ID. The entity must declare it with <see cref="FormPageAttribute"/>; a page that
/// spans entities takes its condition from one of them only.
/// </param>
/// <param name="questionId">
/// The ID of the trigger question. It must be on an earlier page of the same form, and be a
/// <see cref="FormControl.Radio"/>, <see cref="FormControl.Select"/> or
/// <see cref="FormControl.Checkbox"/> on an enum or flags enum property.
/// </param>
/// <param name="values">
/// Members of the trigger's enum, for example <c>YesNoUnknown.Yes</c>. The condition is met when
/// the trigger's answer includes any of them. At least one, with no duplicates.
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
internal sealed class FormPageShowWhenAttribute(
    string pageId,
    string questionId,
    params object[] values
) : Attribute
{
    /// <summary>The ID of the conditional page.</summary>
    public string PageId { get; } = pageId;

    /// <summary>The ID of the trigger question.</summary>
    public string QuestionId { get; } = questionId;

    /// <summary>The trigger answers that make the page apply.</summary>
    public IReadOnlyList<object> Values { get; } = values;
}
