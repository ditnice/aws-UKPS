using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

internal static class SyntheticDataLoader
{
    internal const string ResourceName =
        "UKPS.Api.SyntheticData.synthetic-medicine-records.json.gz";

    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };

    private static readonly Lazy<SyntheticDataSet> _dataSet = new(Load);

    /// <summary>Gets the synthetic data set embedded in this assembly.</summary>
    public static SyntheticDataSet DataSet => _dataSet.Value;

    private static SyntheticDataSet Load()
    {
        using Stream stream =
            typeof(SyntheticDataLoader).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException(
                $"Embedded resource '{ResourceName}' was not found."
            );
        using GZipStream json = new(stream, CompressionMode.Decompress);
        return JsonSerializer.Deserialize<SyntheticDataSet>(json, _options)
            ?? throw new InvalidOperationException("The synthetic data set is empty.");
    }
}
