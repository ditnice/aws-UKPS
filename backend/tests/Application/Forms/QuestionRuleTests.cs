using System.Text.Json.Nodes;
using Shouldly;
using UKPS.Api.Application.Forms.Rules;

namespace UKPS.Api.Tests.Application.Forms;

public class QuestionRuleTests
{
    private static JsonNode? Parse(string json) => JsonNode.Parse(json);

    [Theory]
    [InlineData("\"text\"", true)]
    [InlineData("[\"1\"]", true)]
    [InlineData("null", false)]
    [InlineData("\"\"", false)]
    [InlineData("\"   \"", false)]
    [InlineData("[]", false)]
    public void Required(string json, bool expected) =>
        new RequiredRule("Required").IsSatisfiedBy(Parse(json)).ShouldBe(expected);

    [Theory]
    [InlineData("\"abc\"", true)]
    [InlineData("\"abcd\"", false)]
    [InlineData("null", true)]
    public void MaxLength(string json, bool expected) =>
        new MaxLengthRule(3, "Too long").IsSatisfiedBy(Parse(json)).ShouldBe(expected);

    [Theory]
    [InlineData("[\"1\",\"2\"]", true)]
    [InlineData("[\"1\",\"2\",\"3\"]", false)]
    [InlineData("[]", true)]
    public void MaxItems(string json, bool expected) =>
        new MaxItemsRule(2, "Too many").IsSatisfiedBy(Parse(json)).ShouldBe(expected);
}
