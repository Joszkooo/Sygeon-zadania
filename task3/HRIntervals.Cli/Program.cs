using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using HRIntervals.Core;   // from Core project

static int ShowUsage()
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  HRIntervals.Cli <contracts.json> <reportBegin> <reportEnd> [--out <file>]");
    Console.Error.WriteLine("Example:");
    Console.Error.WriteLine("  HRIntervals.Cli hr-contracts.json 2022-06-15T00:00:00+02:00 2023-04-30T00:00:00+02:00 --out intervals.json");
    return 1;
}

// --- Parse args ---
if (args.Length < 3 || args.Length > 5)
    return ShowUsage();

var contractsPath  = args[0];
var reportBeginRaw = args[1];
var reportEndRaw   = args[2];

string? outPath = null;
if (args.Length == 5)
{
    if (args[3] == "--out")
        outPath = args[4];
    else
        return ShowUsage();
}

// --- Parse report dates ---
DateTimeOffset reportBegin, reportEnd;
try
{
    reportBegin = DateTimeOffset.Parse(reportBeginRaw, CultureInfo.InvariantCulture);
    reportEnd   = DateTimeOffset.Parse(reportEndRaw,   CultureInfo.InvariantCulture);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error parsing report dates: {ex.Message}");
    return 1;
}

// --- Load contracts ---
IReadOnlyList<ContractPeriod> contracts;
try
{
    contracts = ContractLoader.LoadFromFile(contractsPath);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error loading contracts file '{contractsPath}': {ex.Message}");
    return 1;
}

// --- Build intervals ---
List<ReportInterval> intervals;
try
{
    intervals = HRIntervalsService.BuildIntervals(reportBegin, reportEnd, contracts);
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error building intervals: {ex.Message}");
    return 1;
}

// --- Build in-memory JSON-friendly list ---
var dtoList = new List<string[]>(intervals.Count);
foreach (var iv in intervals)
{
    dtoList.Add(new[]
    {
        IntervalFormatting.ToIso(iv.Begin),
        IntervalFormatting.ToIso(iv.End)
    });
}

// --- Serialize with System.Text.Json ---
var json = JsonSerializer.Serialize(dtoList, new JsonSerializerOptions
{
    WriteIndented = true
});

Console.WriteLine(json);

if (!string.IsNullOrWhiteSpace(outPath))
{
    try
    {
        File.WriteAllText(outPath, json);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Warning: could not write output file '{outPath}': {ex.Message}");
    }
}

return 0;
