using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>
/// Reads the form attributes on entity types and pairs each annotated property with its EF
/// metadata. Coverage problems (a mapped column with no decision, misplaced attributes) are
/// reported here; everything else is checked by <see cref="FormModelValidator"/>.
/// </summary>
internal static class FormModelScanner
{
    private static readonly string _annotationNamespace = typeof(FormFieldAttribute).Namespace!;

    public static (IReadOnlyList<EntityDeclaration> Entities, IReadOnlyList<string> Problems) Scan(
        IModel model,
        IEnumerable<Type> types
    )
    {
        List<EntityDeclaration> entities = [];
        List<string> problems = [];

        foreach (Type type in types)
        {
            FormSectionAttribute[] sections = type.GetCustomAttributes<FormSectionAttribute>(false)
                .ToArray();
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
            );

            if (sections.Length == 0)
            {
                problems.AddRange(
                    properties
                        .Where(HasFormAttribute)
                        .Select(p =>
                            $"{type.Name}.{p.Name} has form attributes but {type.Name} has no [FormSection]."
                        )
                );
                continue;
            }

            IEntityType? entityType = model.FindEntityType(type);
            if (entityType is null)
            {
                problems.Add($"{type.Name} has [FormSection] but is not an EF entity.");
                continue;
            }

            List<QuestionDeclaration> questions = [];
            foreach (PropertyInfo property in properties)
            {
                QuestionDeclaration? question = ScanProperty(type, entityType, property, problems);
                if (question is not null)
                {
                    questions.Add(question);
                }
            }

            entities.Add(new EntityDeclaration(type, entityType, sections, questions));
        }

        return (entities, problems);
    }

    private static bool HasFormAttribute(PropertyInfo property) =>
        property
            .GetCustomAttributes(false)
            .Any(a =>
                string.Equals(a.GetType().Namespace, _annotationNamespace, StringComparison.Ordinal)
            );

    private static QuestionDeclaration? ScanProperty(
        Type type,
        IEntityType entityType,
        PropertyInfo property,
        List<string> problems
    )
    {
        string where = $"{type.Name}.{property.Name}";
        FormFieldAttribute? field = property.GetCustomAttribute<FormFieldAttribute>();
        FormTaskAttribute? task = property.GetCustomAttribute<FormTaskAttribute>();
        FormPageAttribute? page = property.GetCustomAttribute<FormPageAttribute>();
        IProperty? column = entityType.FindProperty(property.Name);
        INavigation? navigation = entityType.FindNavigation(property.Name);

        if (!IsCovered(where, property, column, navigation, problems))
        {
            return null;
        }

        if (task is null || page is null)
        {
            if (task is null)
            {
                problems.Add($"{where} has [FormField] but no [FormTask].");
            }

            if (page is null)
            {
                problems.Add($"{where} has [FormField] but no [FormPage].");
            }

            return null;
        }

        BindingInfo? binding = column is not null
            ? DescribeColumn(where, column, problems)
            : DescribeJunction(where, navigation!, problems);

        return binding is null
            ? null
            : new QuestionDeclaration
            {
                EntityType = type,
                Property = property,
                Task = task,
                Page = page,
                Field = field!,
                Required = property.GetCustomAttribute<FormRequiredAttribute>(),
                MaxLength = property.GetCustomAttribute<FormMaxLengthAttribute>(),
                MaxItems = property.GetCustomAttribute<FormMaxItemsAttribute>(),
                Options = property.GetCustomAttribute<FormOptionsAttribute>(),
                Binding = binding,
            };
    }

    /// <summary>
    /// Applies the coverage rule. Returns true when the property is a question to declare.
    /// </summary>
    private static bool IsCovered(
        string where,
        PropertyInfo property,
        IProperty? column,
        INavigation? navigation,
        List<string> problems
    )
    {
        bool hasField = property.IsDefined(typeof(FormFieldAttribute), false);
        bool hasPage = property.IsDefined(typeof(FormPageAttribute), false);
        FormIgnoreAttribute? ignore = property.GetCustomAttribute<FormIgnoreAttribute>();
        bool hasOtherAttributes = HasFormAttribute(property) && !hasField && ignore is null;

        if (
            column is null
            && HasFormAttribute(property)
            && !IsAnnotatableNavigation(where, navigation, ignore, problems)
        )
        {
            return false;
        }

        if (ignore is not null)
        {
            if (string.IsNullOrWhiteSpace(ignore.Reason))
            {
                problems.Add($"{where} has [FormIgnore] with a blank reason.");
            }

            if (hasField)
            {
                problems.Add($"{where} has both [FormField] and [FormIgnore].");
            }
            else if (HasQuestionDetail(property))
            {
                problems.Add($"{where} has [FormIgnore] and question attributes.");
            }

            return false;
        }

        if (!hasField)
        {
            if (column is not null)
            {
                problems.Add(
                    $"{where} is a mapped column with neither [FormField] nor [FormIgnore]."
                );
            }
            else if (hasOtherAttributes)
            {
                problems.Add(
                    hasPage
                        ? $"{where} has [FormPage] but no [FormField]."
                        : $"{where} has question attributes but no [FormField]."
                );
            }

            return false;
        }

        return true;
    }

    private static bool IsAnnotatableNavigation(
        string where,
        INavigation? navigation,
        FormIgnoreAttribute? ignore,
        List<string> problems
    )
    {
        if (navigation is null)
        {
            problems.Add($"{where} has form attributes but is not a mapped column or navigation.");
            return false;
        }

        if (ignore is not null)
        {
            problems.Add($"{where} is a navigation; navigations need no [FormIgnore].");
            return false;
        }

        return true;
    }

    private static bool HasQuestionDetail(PropertyInfo property) =>
        property.IsDefined(typeof(FormPageAttribute), false)
        || property.IsDefined(typeof(FormTaskAttribute), false)
        || property.IsDefined(typeof(FormRequiredAttribute), false)
        || property.IsDefined(typeof(FormMaxLengthAttribute), false)
        || property.IsDefined(typeof(FormMaxItemsAttribute), false)
        || property.IsDefined(typeof(FormOptionsAttribute), false);

    private static BindingInfo? DescribeColumn(
        string where,
        IProperty column,
        List<string> problems
    )
    {
        Type valueType = Nullable.GetUnderlyingType(column.ClrType) ?? column.ClrType;
        IForeignKey? foreignKey = column.GetContainingForeignKeys().FirstOrDefault();

        BindingKind? kind = valueType switch
        {
            _ when valueType == typeof(string) => BindingKind.Text,
            { IsEnum: true } when valueType.IsDefined(typeof(FlagsAttribute), false) =>
                BindingKind.Flags,
            { IsEnum: true } => BindingKind.Enum,
            _ when valueType == typeof(int) && foreignKey is not null => BindingKind.ReferenceId,
            _ => null,
        };

        if (kind is null)
        {
            problems.Add(
                $"{where} has type {valueType.Name}, which no form control supports. Use string, an enum, or an int foreign key to reference data."
            );
            return null;
        }

        return new BindingInfo(kind.Value)
        {
            IsNullable = column.IsNullable,
            EnumType = valueType.IsEnum ? valueType : null,
            ReferenceType =
                kind == BindingKind.ReferenceId ? foreignKey!.PrincipalEntityType.ClrType : null,
        };
    }

    private static BindingInfo? DescribeJunction(
        string where,
        INavigation navigation,
        List<string> problems
    )
    {
        IEntityType junction = navigation.TargetEntityType;
        IForeignKey[] referenceKeys = junction
            .GetForeignKeys()
            .Where(fk => fk != navigation.ForeignKey)
            .ToArray();

        if (
            !navigation.IsCollection
            || referenceKeys.Length != 1
            || referenceKeys[0].Properties.Count != 1
            || referenceKeys[0].Properties[0].ClrType != typeof(int)
            || referenceKeys[0].Properties[0].PropertyInfo is null
        )
        {
            problems.Add(
                $"{where} has form attributes but is not a collection navigation to a junction entity with one int reference foreign key."
            );
            return null;
        }

        return new BindingInfo(BindingKind.Junction)
        {
            ReferenceType = referenceKeys[0].PrincipalEntityType.ClrType,
            JunctionType = junction.ClrType,
            JunctionReferenceIdProperty = referenceKeys[0].Properties[0].PropertyInfo,
        };
    }
}
