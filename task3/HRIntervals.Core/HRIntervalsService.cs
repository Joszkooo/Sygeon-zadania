using System;
using System.Collections.Generic;
using System.Globalization;

namespace HRIntervals.Core;

/// <summary>
/// Builds a sorted list of half-open intervals [Begin, End) inside the report window
/// that mark every change in contract coverage (including gaps = no contract).
/// </summary>
public static class HRIntervalsService
{
    /// <summary>Return Europe/Warsaw (handles Windows/Linux fallback).</summary>
    private static TimeZoneInfo GetWarsawTz()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"); }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
    }

    /// <summary>
    /// Build the list of boundary intervals.
    /// reportBegin/reportEnd can be in *any* offset; will be converted to Warsaw.
    /// </summary>
    public static List<ReportInterval> BuildIntervals(
        DateTimeOffset reportBegin,
        DateTimeOffset reportEnd,
        IEnumerable<ContractPeriod> contracts)
    {
        if (reportEnd <= reportBegin)
            throw new ArgumentException("Report end must be after report begin.");

        var tz = GetWarsawTz();

        // Convert report window to Warsaw
        var rb = TimeZoneInfo.ConvertTime(reportBegin, tz);
        var re = TimeZoneInfo.ConvertTime(reportEnd, tz);

        // Collect boundaries in a set to avoid duplicates
        var boundaries = new HashSet<DateTimeOffset> { rb, re };

        foreach (var c in contracts)
        {
            // Parse from strings in the model
            var cb = c.Begin;                 // DateTimeOffset (raw)
            var ce = c.End;                   // nullable

            // Convert to Warsaw
            var cbW = TimeZoneInfo.ConvertTime(cb, tz);
            var ceW = ce.HasValue ? TimeZoneInfo.ConvertTime(ce.Value, tz) : re; // open-ended clipped at report end

            // Skip if contract ends at/before report begin
            if (ceW <= rb)
                continue;

            // Skip if contract starts at/after report end
            if (cbW >= re)
                continue;

            // Clip to report window
            if (cbW < rb) cbW = rb;
            if (ceW > re) ceW = re;

            boundaries.Add(cbW);
            boundaries.Add(ceW);
        }

        // Sort boundaries
        var list = new List<DateTimeOffset>(boundaries);
        list.Sort();

        // Build result
        var result = new List<ReportInterval>(list.Count - 1);
        for (int i = 0; i < list.Count - 1; i++)
        {
            var begin = list[i];
            var end = list[i + 1];
            if (end > begin) // ignore zero-length
            {
                result.Add(new ReportInterval
                {
                    Begin = begin,
                    End = end
                });
            }
        }

        return result;
    }
}

/// <summary>
/// Simple data carrier for an interval.
/// </summary>
public sealed class ReportInterval
{
    public DateTimeOffset Begin { get; set; }
    public DateTimeOffset End { get; set; }
}

/// <summary>
/// Formatting helpers for printing intervals.
/// </summary>
public static class IntervalFormatting
{
    // Example format: 2022-06-15T00:00:00+02:00
    public const string IsoFormat = "yyyy-MM-dd'T'HH:mm:sszzz";

    public static string ToIso(DateTimeOffset dt) =>
        dt.ToString(IsoFormat, CultureInfo.InvariantCulture);
}
