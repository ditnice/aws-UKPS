using System.Linq.Expressions;
using System.Reflection;
using Shouldly;
using UKPS.Api.Application.RecordForms.Bindings;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Application.RecordForms.Scanning;

namespace UKPS.Api.Tests.Application.RecordForms.Bindings;

/// <summary>
/// Each binding's answer check, compiled and run in memory. The task list runs the same
/// expressions in the database.
/// </summary>
public class HasAnswerExpressionTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("Hepatitis C", true)]
    public void Column_Text_IsAnsweredWhenNotBlank(string? text, bool expected)
    {
        ColumnBinding binding = new(Property(nameof(Row.Text)), ColumnValueKind.Text);

        HasAnswer(binding, new Row { Text = text }).ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(3, true)]
    public void Column_NullableReferenceId_IsAnsweredWhenSet(int? id, bool expected)
    {
        ColumnBinding binding = new(Property(nameof(Row.ReferenceId)), ColumnValueKind.ReferenceId);

        HasAnswer(binding, new Row { ReferenceId = id }).ShouldBe(expected);
    }

    [Fact]
    public void Column_NonNullable_IsAlwaysAnswered()
    {
        ColumnBinding binding = new(Property(nameof(Row.Choice)), ColumnValueKind.Enum);

        HasAnswer(binding, new Row()).ShouldBeTrue();
    }

    [Fact]
    public void Flags_Nullable_IsAnsweredWhenAFlagIsSet()
    {
        FlagsBinding binding = new(Property(nameof(Row.NullableFlags)));

        HasAnswer(binding, new Row { NullableFlags = null }).ShouldBeFalse();
        HasAnswer(binding, new Row { NullableFlags = 0 }).ShouldBeFalse();
        HasAnswer(binding, new Row { NullableFlags = MedicineTechnologyStatus.Biosimilar })
            .ShouldBeTrue();
    }

    [Fact]
    public void Flags_NonNullable_IsAnsweredWhenAFlagIsSet()
    {
        FlagsBinding binding = new(Property(nameof(Row.Flags)));

        HasAnswer(binding, new Row { Flags = default }).ShouldBeFalse();
        HasAnswer(binding, new Row { Flags = PharmaceuticalEntity.Medicines }).ShouldBeTrue();
    }

    [Fact]
    public void Junction_IsAnsweredWhenItHasRows()
    {
        JunctionBinding binding = new(
            Property(nameof(Row.Areas)),
            typeof(ValidEntityArea),
            typeof(ValidEntityArea).GetProperty(nameof(ValidEntityArea.TherapeuticAreaId))!
        );

        HasAnswer(binding, new Row()).ShouldBeFalse();
        HasAnswer(binding, new Row { Areas = [new ValidEntityArea { TherapeuticAreaId = 1 }] })
            .ShouldBeTrue();
    }

    private static PropertyInfo Property(string name) => typeof(Row).GetProperty(name)!;

    private static bool HasAnswer(IFormBinding binding, Row row)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(Row), "row");
        Func<Row, bool> hasAnswer = Expression
            .Lambda<Func<Row, bool>>(binding.HasAnswerExpression(parameter), parameter)
            .Compile();
        return hasAnswer(row);
    }

    private sealed class Row
    {
        public string? Text { get; set; }
        public int? ReferenceId { get; set; }
        public YesNoUnknown Choice { get; set; }
        public MedicineTechnologyStatus? NullableFlags { get; set; }
        public PharmaceuticalEntity Flags { get; set; }
        public ICollection<ValidEntityArea> Areas { get; set; } = [];
    }
}
