using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hosihikari.Bindings.Ir;

public static class IrJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() },
    };

    public static IrManifest ReadManifest(Stream stream) =>
        JsonSerializer.Deserialize<IrManifest>(stream, Options)
        ?? throw new JsonException("IR manifest is empty.");

    public static void WriteManifest(Stream stream, IrManifest manifest) =>
        JsonSerializer.Serialize(stream, manifest, Options);

    public static IReadOnlyList<IrRecord> ReadRecords(Stream stream)
    {
        var records = new List<IrRecord>();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        string? line;
        var lineNumber = 0;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var record = JsonSerializer.Deserialize<IrRecord>(line, Options)
                ?? throw new JsonException($"IR record is empty at line {lineNumber}.");
            records.Add(record);
        }

        return records;
    }

    public static void WriteRecords(Stream stream, IEnumerable<IrRecord> records)
    {
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true)
        {
            NewLine = "\n",
        };
        foreach (var record in records)
        {
            writer.WriteLine(JsonSerializer.Serialize(record, Options));
        }
    }

    public static IReadOnlyList<CoverageEntry> ReadCoverageEntries(Stream stream)
    {
        var entries = new List<CoverageEntry>();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var entry = JsonSerializer.Deserialize<CoverageEntry>(line, Options)
                ?? throw new JsonException("Coverage entry is empty.");
            entries.Add(entry);
        }

        return entries;
    }

    public static void WriteCoverageEntries(Stream stream, IEnumerable<CoverageEntry> entries)
    {
        using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true)
        {
            NewLine = "\n",
        };
        foreach (var entry in entries)
        {
            writer.WriteLine(JsonSerializer.Serialize(entry, Options));
        }
    }

    public static CoverageSummary ReadCoverageSummary(Stream stream) =>
        JsonSerializer.Deserialize<CoverageSummary>(stream, Options)
        ?? throw new JsonException("Coverage summary is empty.");

    public static void WriteCoverageSummary(Stream stream, CoverageSummary summary) =>
        JsonSerializer.Serialize(stream, summary, Options);

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    public static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options)
        ?? throw new JsonException($"Could not deserialize {typeof(T).Name}.");
}
