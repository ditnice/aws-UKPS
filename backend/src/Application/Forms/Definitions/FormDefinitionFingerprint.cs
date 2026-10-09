using System.Security.Cryptography;
using System.Text.Json;

namespace UKPS.Api.Application.Forms.Definitions;

/// <summary>
/// A hash of everything in a definition that affects clients, saved data or audit. A test
/// pins it alongside <see cref="FormDefinition.Version"/> so a change without a version bump
/// fails the build.
/// </summary>
internal static class FormDefinitionFingerprint
{
    public static string Compute(FormDefinition form)
    {
        var description = new
        {
            form.RecordType,
            Sections = form.Sections.Select(section => new
            {
                section.Id,
                section.Title,
                Pages = section.Pages.Select(page => new
                {
                    page.Id,
                    page.Title,
                    Questions = page.Questions.Select(question => new
                    {
                        question.Id,
                        Type = question.Type.ToString(),
                        question.Label,
                        question.Hint,
                        question.Display,
                        Rules = question.Rules.Select(rule => new
                        {
                            rule.Kind,
                            rule.Value,
                            rule.Message,
                        }),
                        Options = question.Options?.Description,
                        Binding = question.Binding.Description,
                    }),
                }),
            }),
        };

        var json = JsonSerializer.SerializeToUtf8Bytes(description);
        return Convert.ToHexStringLower(SHA256.HashData(json));
    }
}
