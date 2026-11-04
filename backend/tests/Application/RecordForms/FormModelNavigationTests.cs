using Shouldly;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>Next and Back stay within a task; each task starts and ends at the task list.</summary>
public class FormModelNavigationTests
{
    private static readonly FormModel _form = new(
        RecordType.Medicine,
        "1",
        [
            Section("first-section", Task("one", "a", "b"), Task("two", "c")),
            Section("second-section", Task("three", "d", "e")),
        ]
    );

    [Theory]
    [InlineData("a", "b")]
    [InlineData("b", null)]
    [InlineData("c", null)]
    [InlineData("d", "e")]
    [InlineData("e", null)]
    public void NextPage_StopsAtTheEndOfTheTask(string pageId, string? expected)
    {
        (_form.NextPage(_form.FindPage(pageId)!)?.Id).ShouldBe(expected);
    }

    [Theory]
    [InlineData("a", null)]
    [InlineData("b", "a")]
    [InlineData("c", null)]
    [InlineData("d", null)]
    [InlineData("e", "d")]
    public void PreviousPage_StopsAtTheStartOfTheTask(string pageId, string? expected)
    {
        (_form.PreviousPage(_form.FindPage(pageId)!)?.Id).ShouldBe(expected);
    }

    private static FormSection Section(string id, params FormTask[] tasks) => new(id, id, 0, tasks);

    private static FormTask Task(string id, params string[] pageIds) =>
        new(
            id,
            id,
            0,
            [.. pageIds.Select(pageId => new FormPage(pageId, pageId, 0, "", "", id, id, []))]
        );
}
