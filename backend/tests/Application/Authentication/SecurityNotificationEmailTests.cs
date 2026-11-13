using System.Globalization;
using System.Text.RegularExpressions;
using Shouldly;
using UKPS.Api.Application.Authentication.SecurityNotification;
using UKPS.Api.Application.InternalServices.Communication;

namespace UKPS.Api.Tests.Application.Authentication;

public sealed class SecurityNotificationEmailTests
{
    private const string HelpDeskEmail = "help@example.com";
    private const string HelpDeskLink = $"""<a href="mailto:{HelpDeskEmail}">{HelpDeskEmail}</a>""";

    public static TheoryData<ChangedUserData> AllUserDataValues =>
        new(Enum.GetValues<ChangedUserData>());

    private static EmailContextData CreateContextData(string helpDeskEmail = HelpDeskEmail) =>
        new() { HelpDeskEmail = helpDeskEmail };

    private static SecurityNotificationEmail CreateEmail(
        ChangedUserData changedValue = ChangedUserData.Password
    ) => new() { ChangedValue = changedValue };

    [Fact]
    public void Subject_WhenPasswordChanged_ReturnsLowercasedValueInSubject()
    {
        var email = CreateEmail(ChangedUserData.Password);

        email.Subject.ShouldBe("Your password was changed");
    }

    [Fact]
    public void GetHtmlContent_WhenPasswordChanged_ContainsLowercasedValue()
    {
        var email = CreateEmail(ChangedUserData.Password);

        var html = email.GetHtmlContent(CreateContextData());

        html.ShouldContain("<p>Your password was recently changed.</p>");
    }

    [Fact]
    public void GetHtmlContent_ContainsNoActionNeededMessage()
    {
        var html = CreateEmail().GetHtmlContent(CreateContextData());

        html.ShouldContain("<p>If you made this change, no further action is needed.</p>");
    }

    [Fact]
    public void GetHtmlContent_ContainsHelpDeskEmailLink()
    {
        var html = CreateEmail().GetHtmlContent(CreateContextData());

        html.ShouldContain($"<p>If you didn't, please contact support at {HelpDeskLink}.</p>");
    }

    [Fact]
    public void GetHtmlContent_ReturnsThreeParagraphs()
    {
        var html = CreateEmail().GetHtmlContent(CreateContextData());

        Regex.Count(html, "<p>").ShouldBe(3);
        Regex.Count(html, "</p>").ShouldBe(3);
    }

    [Fact]
    public void GetHtmlContent_WithDifferentHelpDeskEmail_RendersThatEmail()
    {
        const string otherEmail = "other@example.com";

        var html = CreateEmail().GetHtmlContent(CreateContextData(otherEmail));

        html.ShouldContain($"""<a href="mailto:{otherEmail}">{otherEmail}</a>""");
        html.ShouldNotContain(HelpDeskEmail);
    }

    // Guards against new UserData values being added without the email still reading correctly
    [Theory]
    [MemberData(nameof(AllUserDataValues))]
    public void SubjectAndHtml_ForEveryUserDataValue_UseLowercasedName(ChangedUserData changedValue)
    {
        var expected = changedValue.ToString().ToLower(CultureInfo.CurrentCulture);
        var email = CreateEmail(changedValue);

        email.Subject.ShouldBe($"Your {expected} was changed");
        email
            .GetHtmlContent(CreateContextData())
            .ShouldContain($"Your {expected} was recently changed.");
    }
}
