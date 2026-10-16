using System.Text.Json.Nodes;
using Shouldly;
using UKPS.Api.Application.Forms.Definitions;

namespace UKPS.Api.Tests.Application.Forms;

public class QuestionTypeTests
{
    [Theory]
    [InlineData(nameof(QuestionType.Textarea), "\"text\"", true)]
    [InlineData(nameof(QuestionType.Textarea), "null", true)]
    [InlineData(nameof(QuestionType.Textarea), "1", false)]
    [InlineData(nameof(QuestionType.Radio), "[\"Yes\"]", false)]
    [InlineData(nameof(QuestionType.Select), "true", false)]
    [InlineData(nameof(QuestionType.Checkbox), "[\"1\",\"2\"]", true)]
    [InlineData(nameof(QuestionType.Checkbox), "[]", true)]
    [InlineData(nameof(QuestionType.Checkbox), "null", false)]
    [InlineData(nameof(QuestionType.Checkbox), "\"1\"", false)]
    [InlineData(nameof(QuestionType.Checkbox), "[1]", false)]
    [InlineData(nameof(QuestionType.Checkbox), "[null]", false)]
    [InlineData(nameof(QuestionType.Checkbox), "[\"1\",\"1\"]", false)]
    public void IsValidShape(string type, string json, bool expected) =>
        Enum.Parse<QuestionType>(type).IsValidShape(JsonNode.Parse(json)).ShouldBe(expected);

    [Theory]
    [InlineData("\"  padded  \"", "\"padded\"")]
    [InlineData("\"   \"", "null")]
    [InlineData("\"\"", "null")]
    [InlineData("null", "null")]
    public void Normalise_Textarea_TrimsAndBlanksToNull(string json, string expected)
    {
        var normalised = QuestionType.Textarea.Normalise(JsonNode.Parse(json));

        (normalised?.ToJsonString() ?? "null").ShouldBe(expected);
    }

    [Fact]
    public void Normalise_Radio_LeavesValueAlone()
    {
        var normalised = QuestionType.Radio.Normalise(JsonValue.Create(" Yes "));

        normalised!.GetValue<string>().ShouldBe(" Yes ");
    }

    [Theory]
    [InlineData("null", null)]
    [InlineData("[]", null)]
    [InlineData("\"Unknown\"", "Unknown")]
    [InlineData("[\"4\",\"7\"]", "[\"4\",\"7\"]")]
    public void ToAuditValue(string json, string? expected) =>
        AnswerValues.ToAuditValue(JsonNode.Parse(json)).ShouldBe(expected);
}
