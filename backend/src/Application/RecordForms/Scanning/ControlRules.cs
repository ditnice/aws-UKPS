using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Which controls suit which bindings, and which is inferred when none is set.</summary>
internal static class ControlRules
{
    public static FormControl? Infer(BindingKind kind) =>
        kind switch
        {
            BindingKind.Text => FormControl.Text,
            BindingKind.Enum => FormControl.Radio,
            BindingKind.Flags or BindingKind.Junction => FormControl.Checkbox,
            _ => null,
        };

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
}
