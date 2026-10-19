using System.Collections;
using System.Globalization;
using System.Reflection;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Bindings;

/// <summary>
/// Stores a multi-value answer as rows of a junction entity, one per chosen reference ID. Writes
/// insert and delete only the difference between the stored and submitted sets.
/// </summary>
internal sealed class JunctionBinding : IFormBinding
{
    private readonly PropertyInfo _navigation;
    private readonly Type _junctionType;
    private readonly PropertyInfo _referenceIdProperty;
    private readonly MethodInfo _add;
    private readonly MethodInfo _remove;

    public JunctionBinding(
        PropertyInfo navigation,
        Type junctionType,
        PropertyInfo referenceIdProperty
    )
    {
        _navigation = navigation;
        _junctionType = junctionType;
        _referenceIdProperty = referenceIdProperty;
        Type collectionType = typeof(ICollection<>).MakeGenericType(junctionType);
        _add = collectionType.GetMethod(nameof(ICollection<>.Add))!;
        _remove = collectionType.GetMethod(nameof(ICollection<>.Remove))!;
    }

    public AnswerShape Shape => AnswerShape.Multi;

    public string NavigationName => _navigation.Name;

    public string Description =>
        $"junction:{_navigation.DeclaringType!.Name}.{_navigation.Name}:{_junctionType.Name}.{_referenceIdProperty.Name}";

    public FormAnswer Read(object? row) =>
        FormAnswer.Multi(
            Items(row)
                .Select(item =>
                    ((int)_referenceIdProperty.GetValue(item)!).ToString(
                        CultureInfo.InvariantCulture
                    )
                )
        );

    public void Write(AppDbContext dbContext, object row, FormAnswer answer)
    {
        object collection = _navigation.GetValue(row)!;
        HashSet<int> wanted = answer
            .Values.Select(static v => int.Parse(v, CultureInfo.InvariantCulture))
            .ToHashSet();

        foreach (object item in Items(row).ToArray())
        {
            int id = (int)_referenceIdProperty.GetValue(item)!;
            if (!wanted.Remove(id))
            {
                _remove.Invoke(collection, [item]);
                dbContext.Remove(item);
            }
        }

        foreach (int id in wanted)
        {
            object item = Activator.CreateInstance(_junctionType)!;
            _referenceIdProperty.SetValue(item, id);
            _add.Invoke(collection, [item]);
        }
    }

    private IEnumerable<object> Items(object? row) =>
        row is null || _navigation.GetValue(row) is not IEnumerable items
            ? []
            : items.Cast<object>();
}
