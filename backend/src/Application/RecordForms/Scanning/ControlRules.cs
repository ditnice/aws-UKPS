using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Which controls suit which bindings, and which provider a choice control needs.</summary>
internal static class ControlRules
{
    public static IReadOnlyList<FormControl> Allowed(BindingKind kind) =>
        kind switch
        {
            BindingKind.Text => [FormControl.Text, FormControl.Textarea],
            BindingKind.Enum => [FormControl.Radio, FormControl.Select],
            BindingKind.ReferenceId =>
            [
                FormControl.Radio,
                FormControl.Select,
                FormControl.HierarchicalSelect,
            ],
            BindingKind.Flags => [FormControl.Checkbox],
            BindingKind.Junction => [FormControl.Checkbox, FormControl.HierarchicalSelect],
            _ => [],
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
