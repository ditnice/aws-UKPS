using System.Text.Json;
using System.Text.Json.Serialization;
using Shouldly;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// Runs the date answer cases shared with the frontend, so the two date parsers cannot drift.
/// </summary>
public class DateCasesTests
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly DateCaseFile _file = LoadFile();

    public static TheoryData<string> CaseNames() => new(_file.Cases.Select(static c => c.Name));

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void DateCase_GivesTheExpectedError(string name)
    {
        DateCase dateCase = _file.Cases.Single(c =>
            string.Equals(c.Name, name, StringComparison.Ordinal)
        );

        DateAnswer.Check(dateCase.Answer, _file.Today).ShouldBe(dateCase.Error);
        if (dateCase.Precision is null)
        {
            return;
        }

        DateAnswer
            .TryParse(dateCase.Answer, out DatePrecision precision, out DateOnly value)
            .ShouldBeTrue();
        precision.ShouldBe(dateCase.Precision.Value);
        DateAnswer.Format(precision, value).ShouldBe(dateCase.Answer);
        DateAnswer.PrecisionOf(dateCase.Answer).ShouldBe(precision.ToString());
    }

    [Fact]
    public void UkToday_IsTheDateInTheUk()
    {
        // 23:30 UTC is already the next day in British Summer Time, but not in winter.
        DateAnswer
            .UkToday(new DateTime(2026, 6, 15, 23, 30, 0, DateTimeKind.Utc))
            .ShouldBe(new DateOnly(2026, 6, 16));
        DateAnswer
            .UkToday(new DateTime(2026, 12, 15, 23, 30, 0, DateTimeKind.Utc))
            .ShouldBe(new DateOnly(2026, 12, 15));
    }

    [Fact]
    public void Format_StoresQuartersAndMonthsAsTheirFirstDay()
    {
        DateAnswer.TryParse("2027-Q3", out _, out DateOnly quarter).ShouldBeTrue();
        quarter.ShouldBe(new DateOnly(2027, 7, 1));
        DateAnswer
            .Format(DatePrecision.EstimatedQuarter, new DateOnly(2027, 9, 30))
            .ShouldBe("2027-Q3");
        DateAnswer.TryParse("2027-11", out _, out DateOnly month).ShouldBeTrue();
        month.ShouldBe(new DateOnly(2027, 11, 1));
    }

    private static DateCaseFile LoadFile()
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Application",
            "RecordForms",
            "Fixtures",
            "date-cases.json"
        );
        return JsonSerializer.Deserialize<DateCaseFile>(File.ReadAllText(path), _jsonOptions)!;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812",
        Justification = "Created by JSON deserialisation."
    )]
    private sealed record DateCaseFile(DateOnly Today, IReadOnlyList<DateCase> Cases);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1812",
        Justification = "Created by JSON deserialisation."
    )]
    private sealed record DateCase(
        string Name,
        string Answer,
        string? Error,
        DatePrecision? Precision
    );
}
