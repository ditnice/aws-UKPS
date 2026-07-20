using System.Text.Json;
using System.Text.Json.Serialization;

namespace UKPS.Api.Tests.Utilities.Fixtures;

/// <summary>
/// Shared JSON options for HTTP integration tests, mirroring the JsonStringEnumConverter +
/// web/camelCase defaults configured in Program.cs.
/// </summary>
internal static class TestJsonOptions
{
    public static JsonSerializerOptions Default { get; } =
        new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
        };
}
