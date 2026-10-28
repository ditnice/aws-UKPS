using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UKPS.Api.Application.RecordForms.Model;
using UKPS.Api.Application.RecordForms.Options;

namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>
/// Hashes everything in a form model that clients, saved data or audit depend on. A test pins it
/// next to <see cref="FormVersions"/>, so changing a form attribute without bumping the version
/// fails the build.
/// </summary>
internal static class FormModelFingerprint
{
    public static string Compute(FormModel model, OptionProviderRegistry options) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Describe(model, options))));

    /// <summary>The canonical text the fingerprint hashes; readable, so a test can print it.</summary>
    public static string Describe(FormModel model, OptionProviderRegistry options)
    {
        StringBuilder text = new();
        foreach (FormSection section in model.Sections)
        {
            text.AppendLine(
                CultureInfo.InvariantCulture,
                $"section {section.Id} | {section.Title} | {section.Order}"
            );
            foreach (FormTask task in section.Tasks)
            {
                text.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"  task {task.Id} | {task.Title} | {task.Order}"
                );
                foreach (FormPage page in task.Pages)
                {
                    text.AppendLine(
                        CultureInfo.InvariantCulture,
                        $"    page {page.Id} | {page.Title} | {page.Order}"
                    );
                    foreach (FormQuestion question in page.Questions)
                    {
                        AppendQuestion(text, question, options);
                    }
                }
            }
        }

        return text.ToString();
    }

    private static void AppendQuestion(
        StringBuilder text,
        FormQuestion question,
        OptionProviderRegistry options
    )
    {
        text.AppendLine(
            CultureInfo.InvariantCulture,
            $"    question {question.Id} | {question.Control} | {question.Label} | {question.Hint} | {question.Display} | {question.Order}"
        );
        text.AppendLine(
            CultureInfo.InvariantCulture,
            $"      text {question.Placeholder} | {question.ChildLabel} | {question.ChildPlaceholder} | {question.AddAnotherLabel}"
        );
        text.AppendLine(
            CultureInfo.InvariantCulture,
            $"      binding {question.Binding.Description} | {question.Entity.RowSource}"
        );
        foreach (FormRule rule in question.Rules)
        {
            text.AppendLine(
                CultureInfo.InvariantCulture,
                $"      rule {rule.Kind} | {rule.Value} | {rule.Message}"
            );
        }

        if (question.OptionKey is not null)
        {
            text.AppendLine(
                CultureInfo.InvariantCulture,
                $"      options {question.OptionKey} | {options.Find(question.OptionKey)?.Description}"
            );
        }
    }
}
