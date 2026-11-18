using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
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

    public static (FormDeclarations Declarations, IReadOnlyList<string> Problems) Scan(
        IModel model,
        IEnumerable<Type> types
    )
    {
        List<FormLayout> layouts = [];
        List<EntityDeclaration> entities = [];
        List<string> problems = [];
        Type[] allTypes = types.ToArray();
        HashSet<Type> groupItemTypes = allTypes
            .Where(static t => t.IsDefined(typeof(FormEntityAttribute), false))
            .SelectMany(GroupItemTypes)
            .ToHashSet();

        foreach (Type type in allTypes)
        {
            if (type.IsSubclassOf(typeof(FormLayout)) && !type.IsAbstract)
            {
                if (CreateLayout(type, problems) is { } layout)
                {
                    layouts.Add(layout);
                }
            }
            else if (ScanEntity(model, type, groupItemTypes, problems) is { } entity)
            {
                entities.Add(entity);
            }
        }

        return (new FormDeclarations(layouts, entities), problems);
    }

    private static EntityDeclaration? ScanEntity(
        IModel model,
        Type type,
        HashSet<Type> groupItemTypes,
        List<string> problems
    )
    {
        FormEntityAttribute? formEntity = type.GetCustomAttribute<FormEntityAttribute>(false);
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
        );

        FormPageShowWhenAttribute[] pageConditions =
            type.GetCustomAttributes<FormPageShowWhenAttribute>(false).ToArray();

        if (formEntity is null)
        {
            // A group's child entity is scanned through the group that holds it.
            if (!groupItemTypes.Contains(type))
            {
                ReportUnannotated(type, properties, pageConditions, problems);
            }

            return null;
        }

        IEntityType? entityType = model.FindEntityType(type);
        if (entityType is null)
        {
            problems.Add($"{type.Name} has [FormEntity] but is not an EF entity.");
            return null;
        }

        QuestionDeclaration[] questions = properties
            .Select(property => ScanProperty(type, entityType, property, parent: null, problems))
            .OfType<QuestionDeclaration>()
            .ToArray();

        return new EntityDeclaration(
            type,
            entityType,
            formEntity.RowSource,
            questions,
            pageConditions
        );
    }

    private static void ReportUnannotated(
        Type type,
        PropertyInfo[] properties,
        FormPageShowWhenAttribute[] pageConditions,
        List<string> problems
    )
    {
        if (pageConditions.Length > 0)
        {
            problems.Add($"{type.Name} has [FormPageShowWhen] but no [FormEntity].");
        }

        problems.AddRange(
            properties
                .Where(HasFormAttribute)
                .Select(p =>
                    $"{type.Name}.{p.Name} has form attributes but {type.Name} has no [FormEntity]."
                )
        );
    }

    private static FormLayout? CreateLayout(Type type, List<string> problems)
    {
        ConstructorInfo? constructor = type.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            Type.EmptyTypes
        );
        if (constructor is null)
        {
            problems.Add($"{type.Name} is a FormLayout with no parameterless constructor.");
            return null;
        }

        try
        {
            return (FormLayout)constructor.Invoke(null);
        }
        catch (TargetInvocationException exception)
        {
            problems.Add(
                $"{type.Name} threw while being created: {exception.InnerException?.Message}"
            );
            return null;
        }
    }

    /// <summary>The child entities of the type's group questions.</summary>
    private static IEnumerable<Type> GroupItemTypes(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(static p =>
                p.GetCustomAttribute<FormFieldAttribute>()?.Control == FormControl.Group
            )
            .Select(static p => CollectionItemType(p.PropertyType))
            .OfType<Type>();

    private static Type? CollectionItemType(Type type) =>
        type.IsGenericType && type.GetGenericArguments() is [Type item] ? item : null;

    private static bool HasFormAttribute(PropertyInfo property) =>
        property
            .GetCustomAttributes(false)
            .Any(a =>
                string.Equals(a.GetType().Namespace, _annotationNamespace, StringComparison.Ordinal)
            );

    /// <summary>
    /// Reads one property. A group's child questions pass the group as <paramref name="parent"/>:
    /// they take its page, and must not declare their own.
    /// </summary>
    private static QuestionDeclaration? ScanProperty(
        Type type,
        IEntityType entityType,
        PropertyInfo property,
        QuestionDeclaration? parent,
        List<string> problems
    )
    {
        string where = $"{type.Name}.{property.Name}";
        FormFieldAttribute? field = property.GetCustomAttribute<FormFieldAttribute>();
        FormPageAttribute? page = property.GetCustomAttribute<FormPageAttribute>();
        FormListSourceAttribute? listSource =
            property.GetCustomAttribute<FormListSourceAttribute>();
        IProperty? column = entityType.FindProperty(property.Name);
        INavigation? navigation = listSource is null
            ? entityType.FindNavigation(property.Name)
            : FindListSource(where, entityType, property, listSource, problems);
        if (
            (listSource is not null && navigation is null)
            || !IsCovered(where, property, column, navigation, problems)
        )
        {
            return null;
        }

        string? pageId = PlaceQuestion(where, page, parent, problems);
        if (pageId is null)
        {
            return null;
        }

        if (
            DescribeBinding(where, field!.Control, column, navigation, listSource, problems)
            is not { } binding
        )
        {
            return null;
        }

        QuestionDeclaration question = new()
        {
            EntityType = type,
            Property = property,
            PageId = pageId,
            Field = field,
            Required = property.GetCustomAttribute<FormRequiredAttribute>(),
            MaxLength = property.GetCustomAttribute<FormMaxLengthAttribute>(),
            MaxItems = property.GetCustomAttribute<FormMaxItemsAttribute>(),
            Prefixes = property.GetCustomAttribute<FormValuePrefixesAttribute>(),
            Options = property.GetCustomAttribute<FormOptionsAttribute>(),
            ShowWhen = property.GetCustomAttribute<FormShowWhenAttribute>(),
            DateEvent = property.GetCustomAttribute<FormDateEventAttribute>(),
            Distinct = property.GetCustomAttribute<FormDistinctAttribute>(),
            Binding = binding,
            IsGroupChild = parent is not null,
        };

        return binding.Kind == BindingKind.Group
            ? question with
            {
                Children = ScanGroupChildren(question, navigation!.TargetEntityType, problems),
            }
            : question;
    }

    /// <summary>
    /// The page ID of a question: its own, or for a group's question the group's, which it must
    /// not declare itself. Null when it is missing.
    /// </summary>
    private static string? PlaceQuestion(
        string where,
        FormPageAttribute? page,
        QuestionDeclaration? parent,
        List<string> problems
    )
    {
        if (parent is not null)
        {
            if (page is not null)
            {
                problems.Add(
                    $"{where} is a question of group {parent.Where}, so it takes the group's page and must not declare [FormPage]."
                );
            }

            return parent.PageId;
        }

        if (page is null)
        {
            problems.Add($"{where} has [FormField] but no [FormPage].");
        }

        return page?.Id;
    }

    /// <summary>The questions declared on a group's child entity.</summary>
    private static QuestionDeclaration[] ScanGroupChildren(
        QuestionDeclaration group,
        IEntityType childType,
        List<string> problems
    )
    {
        List<QuestionDeclaration> children = [];
        foreach (
            PropertyInfo property in childType.ClrType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
            )
        )
        {
            QuestionDeclaration? child = ScanProperty(
                childType.ClrType,
                childType,
                property,
                group,
                problems
            );
            if (child is not null)
            {
                children.Add(child);
            }
        }

        if (children.Count == 0)
        {
            problems.Add(
                $"{group.Where} is a group, but {childType.ClrType.Name} has no questions."
            );
        }

        return [.. children];
    }

    /// <summary>Where a covered property's answer is stored, worked out from its EF metadata.</summary>
    private static BindingInfo? DescribeBinding(
        string where,
        FormControl control,
        IProperty? column,
        INavigation? navigation,
        FormListSourceAttribute? listSource,
        List<string> problems
    )
    {
        if (listSource is not null && control != FormControl.TextList)
        {
            problems.Add($"{where} has [FormListSource], which only a TextList uses.");
            return null;
        }

        return (column, control) switch
        {
            ({ }, _) => DescribeColumn(where, column, problems),
            (null, FormControl.Group) => DescribeChildren(where, navigation!, problems),
            (null, FormControl.TextList) => WithListSource(
                where,
                DescribeTextList(where, navigation!, problems),
                navigation!,
                listSource,
                problems
            ),
            _ => DescribeJunction(where, navigation!, problems),
        };
    }

    /// <summary>
    /// The navigation a <c>[FormListSource]</c> names, which must be on an unmapped property.
    /// </summary>
    private static INavigation? FindListSource(
        string where,
        IEntityType entityType,
        PropertyInfo property,
        FormListSourceAttribute listSource,
        List<string> problems
    )
    {
        if (
            entityType.FindProperty(property.Name) is not null
            || entityType.FindNavigation(property.Name) is not null
        )
        {
            problems.Add(
                $"{where} has [FormListSource] but is mapped by EF; put it on an unmapped property."
            );
            return null;
        }

        INavigation? source = entityType.FindNavigation(listSource.Navigation);
        if (source is null)
        {
            problems.Add(
                $"{where} has [FormListSource] naming '{listSource.Navigation}', which is not a navigation of {entityType.ClrType.Name}."
            );
        }

        return source;
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
        || property.IsDefined(typeof(FormRequiredAttribute), false)
        || property.IsDefined(typeof(FormMaxLengthAttribute), false)
        || property.IsDefined(typeof(FormValuePrefixesAttribute), false)
        || property.IsDefined(typeof(FormMaxItemsAttribute), false)
        || property.IsDefined(typeof(FormOptionsAttribute), false)
        || property.IsDefined(typeof(FormShowWhenAttribute), false)
        || property.IsDefined(typeof(FormDateEventAttribute), false)
        || property.IsDefined(typeof(FormDistinctAttribute), false)
        || property.IsDefined(typeof(FormListSourceAttribute), false);

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
            _ when valueType == typeof(int)
                    && foreignKey?.PrincipalEntityType.ClrType == typeof(RegulatoryDate) =>
                BindingKind.RegulatoryDate,
            _ when valueType == typeof(int) && foreignKey is not null => BindingKind.ReferenceId,
            _ => null,
        };

        if (kind is null)
        {
            problems.Add(
                $"{where} has type {valueType.Name}, which no form control supports. Use string, an enum, or an int foreign key to reference data or a regulatory date."
            );
            return null;
        }

        PropertyInfo? dateNavigation = null;
        if (kind == BindingKind.RegulatoryDate)
        {
            dateNavigation = foreignKey!.DependentToPrincipal?.PropertyInfo;
            if (dateNavigation is null)
            {
                problems.Add(
                    $"{where} is a foreign key to RegulatoryDate with no navigation to load the date through."
                );
                return null;
            }
        }

        return new BindingInfo(kind.Value)
        {
            IsNullable = column.IsNullable,
            EnumType = valueType.IsEnum ? valueType : null,
            ReferenceType =
                kind == BindingKind.ReferenceId ? foreignKey!.PrincipalEntityType.ClrType : null,
            DateNavigation = dateNavigation,
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

    private static BindingInfo? DescribeChildren(
        string where,
        INavigation navigation,
        List<string> problems
    )
    {
        BindingInfo? children = DescribeChildCollection(where, navigation, "Group", problems);
        return children is null ? null : children with { Kind = BindingKind.Group };
    }

    private static BindingInfo? DescribeTextList(
        string where,
        INavigation navigation,
        List<string> problems
    )
    {
        BindingInfo? children = DescribeChildCollection(where, navigation, "TextList", problems);
        if (children is null)
        {
            return null;
        }

        IProperty[] values = navigation
            .TargetEntityType.GetProperties()
            .Where(static p => p.ClrType == typeof(string) && !p.IsKey() && !p.IsForeignKey())
            .ToArray();
        if (values is not [{ PropertyInfo: { } value }])
        {
            problems.Add(
                $"{where} uses TextList, but {navigation.TargetEntityType.ClrType.Name} does not have exactly one string column to hold each entry."
            );
            return null;
        }

        return children with
        {
            Kind = BindingKind.TextList,
            ChildValueProperty = value,
        };
    }

    /// <summary>
    /// The child entity of a collection navigation, its int primary key and its optional
    /// <c>int? DisplayOrder</c>.
    /// </summary>
    private static BindingInfo? DescribeChildCollection(
        string where,
        INavigation navigation,
        string control,
        List<string> problems
    )
    {
        IEntityType child = navigation.TargetEntityType;
        IReadOnlyList<IProperty>? key = child.FindPrimaryKey()?.Properties;
        if (
            !navigation.IsCollection
            || key is not [{ PropertyInfo: { } id }]
            || id.PropertyType != typeof(int)
        )
        {
            problems.Add(
                $"{where} uses {control}, which needs a collection navigation to an entity with an int primary key."
            );
            return null;
        }

        IProperty? displayOrder = child.FindProperty("DisplayOrder");
        if (displayOrder is not null && displayOrder.ClrType != typeof(int?))
        {
            problems.Add(
                $"{where} uses {control}, but {child.ClrType.Name}.DisplayOrder is not an int?."
            );
            return null;
        }

        return new BindingInfo(BindingKind.Group)
        {
            ChildType = child.ClrType,
            ChildIdProperty = id,
            ChildDisplayOrderProperty = displayOrder?.PropertyInfo,
        };
    }

    /// <summary>
    /// A text list's collection, and for a list sharing it with others (<c>[FormListSource]</c>),
    /// the child's column that holds the list's kind.
    /// </summary>
    private static BindingInfo? WithListSource(
        string where,
        BindingInfo? textList,
        INavigation navigation,
        FormListSourceAttribute? listSource,
        List<string> problems
    )
    {
        if (textList is null || listSource is null)
        {
            return textList is null
                ? null
                : textList with
                {
                    ChildNavigation = navigation.PropertyInfo,
                };
        }

        PropertyInfo? kindProperty = FindKindColumn(
            where,
            navigation.TargetEntityType,
            listSource,
            problems
        );
        return kindProperty is null
            ? null
            : textList with
            {
                ChildNavigation = navigation.PropertyInfo,
                ChildKindProperty = kindProperty,
                ChildKind = listSource.Kind,
            };
    }

    /// <summary>The child's one column of the <c>[FormListSource]</c> kind's enum type.</summary>
    private static PropertyInfo? FindKindColumn(
        string where,
        IEntityType child,
        FormListSourceAttribute listSource,
        List<string> problems
    )
    {
        Type kindType = listSource.Kind.GetType();
        IProperty[] kindColumns = child
            .GetProperties()
            .Where(p => kindType.IsEnum && p.ClrType == kindType)
            .ToArray();
        if (kindColumns is [{ PropertyInfo: { } kindColumn }])
        {
            return kindColumn;
        }

        problems.Add(
            $"{where} has [FormListSource] kind {listSource.Kind}, but {child.ClrType.Name} does not have exactly one non-nullable {kindType.Name} enum column to hold it."
        );
        return null;
    }
}
