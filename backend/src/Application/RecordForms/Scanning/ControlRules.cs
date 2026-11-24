using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Which controls suit which bindings, and which provider a choice control needs.</summary>
internal static class ControlRules
{
    public static IReadOnlyList<FormControl> Allowed(BindingKind kind) =>
        kind switch
        {
            BindingKind.Text =>
            [
                FormControl.Text,
                FormControl.Textarea,
                FormControl.PrefixedNumber,
            ],
            BindingKind.Enum => [FormControl.Radio, FormControl.Select],
            BindingKind.ReferenceId =>
            [
                FormControl.Radio,
                FormControl.Select,
                FormControl.HierarchicalSelect,
            ],
            BindingKind.Flags => [FormControl.Checkbox],
            BindingKind.Junction => [FormControl.Checkbox, FormControl.HierarchicalSelect],
            BindingKind.RegulatoryDate =>
            [
                FormControl.PrecisionDate,
                FormControl.ActualMonth,
                FormControl.ActualDate,
            ],
            BindingKind.Group => [FormControl.Group],
            BindingKind.TextList => [FormControl.TextList],
            _ => [],
        };

    /// <summary>
    /// Whether the control's answer is a date, as for a regulatory date binding; mirrored by
    /// <c>isDateControl</c> in <c>frontend/src/lib/dynamic-form/dates.ts</c>.
    /// </summary>
    public static bool IsDate(FormControl control) =>
        Allowed(BindingKind.RegulatoryDate).Contains(control);

    /// <summary>The controls a group's child question may use.</summary>
    public static bool IsAllowedInGroup(FormControl control) =>
        control
            is FormControl.Text
                or FormControl.Textarea
                or FormControl.PrefixedNumber
                or FormControl.TextList;

    /// <summary>Whether the control takes free text, so its answer has a maximum length.</summary>
    public static bool IsFreeText(FormControl control) => DefaultMaxLength(control) is not null;

    /// <summary>
    /// The most UTF-16 code units a free text answer (each entry, for a text list) may have when
    /// its question has no <see cref="FormMaxLengthAttribute"/>; null for other controls.
    /// </summary>
    public static int? DefaultMaxLength(FormControl control) =>
        control switch
        {
            FormControl.Text or FormControl.TextList => 2000,
            FormControl.Textarea => 20000,
            _ => null,
        };

    public static bool IsChoice(FormControl control) =>
        control
            is FormControl.Radio
                or FormControl.Select
                or FormControl.Checkbox
                or FormControl.HierarchicalSelect;

    /// <summary>The kind and source type of provider a choice question needs.</summary>
    public static (OptionProviderKind Kind, Type? Source) ExpectedProvider(
        BindingInfo binding,
        FormControl control
    )
    {
        if (binding.Kind is BindingKind.Enum or BindingKind.Flags)
        {
            return (OptionProviderKind.Enum, binding.EnumType);
        }

        OptionProviderKind kind =
            control == FormControl.HierarchicalSelect
                ? OptionProviderKind.ReferenceTree
                : OptionProviderKind.Reference;
        return (kind, binding.ReferenceType);
    }
}
