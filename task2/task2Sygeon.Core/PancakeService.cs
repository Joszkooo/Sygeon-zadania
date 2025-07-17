using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace task2Sygeon.Core;

public static class PancakeAggregator
{
    // create JSON options once (keep names as-is)
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    /// <summary>
    /// Main helper: takes JSON text, returns aggregated rows.
    /// </summary>
    public static List<AggregatedRow> AggregateJson(string json)
    {
        var rows = JsonSerializer.Deserialize<List<UsageRecord>>(json, _jsonOptions)
            ?? new List<UsageRecord>();
        return Aggregate(rows);
    }

    /// <summary>
    /// Aggregate already-deserialized records.
    /// </summary>
    public static List<AggregatedRow> Aggregate(IEnumerable<UsageRecord> records)
    {
        var tz = GetWarsawTimeZone();

        // bucket key = DateTimeOffset floored to hour in Warsaw
        var buckets = new Dictionary<DateTimeOffset, (decimal flour, decimal groat, decimal milk, decimal egg)>();

        foreach (var r in records)
        {
            if (!DateTimeOffset.TryParse(r.TIMESTAMP, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var instant))
            {
                // skip bad rows (junior-friendly: we log to console; in production we might throw)
                Console.Error.WriteLine($"Skipping invalid timestamp: {r.TIMESTAMP}");
                continue;
            }

            // convert to Warsaw
            var local = TimeZoneInfo.ConvertTime(instant, tz);

            // floor to hour
            var hourStart = new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, 0, 0, local.Offset);

            if (!buckets.TryGetValue(hourStart, out var agg)) agg = default;
            agg.flour += r.FLOUR; // still in dkg
            agg.groat += r.GROAT; // g
            agg.milk  += r.MILK;  // ml
            agg.egg   += r.EGG;   // pieces
            buckets[hourStart] = agg;
        }

        // project to list (sorted by time)
        var list = new List<AggregatedRow>(buckets.Count);
        foreach (var kvp in buckets.OrderBy(k => k.Key))
        {
            var k = kvp.Key;
            var v = kvp.Value;
            list.Add(new AggregatedRow
            {
                TIMESTAMP = k.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture),
                FLOUR_KG = Round2(v.flour / 100m),    // dkg -> kg
                GROAT_KG = Round2(v.groat / 1000m),   // g -> kg
                MILK_L   = Round2(v.milk  / 1000m),   // ml -> L
                EGG_PCS  = Round2(v.egg)              // keep as decimal so rounding still 2dp if needed
            });
        }
        return list;
    }

    private static decimal Round2(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    // Cross-platform lookup: Windows uses tz id "Central European Standard Time", Linux/macOS use "Europe/Warsaw".
    private static TimeZoneInfo GetWarsawTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"); }
        catch
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"); }
            catch { return TimeZoneInfo.Utc; } // last resort
        }
    }

    /// <summary>Serialize aggregated rows back to JSON.</summary>
    public static string ToJson(IEnumerable<AggregatedRow> rows, bool indented = false)
    {
        var opts = new JsonSerializerOptions(_jsonOptions) { WriteIndented = indented };
        return JsonSerializer.Serialize(rows, opts);
    }
}