using System;
using System.Text.Json.Serialization;

namespace HRIntervals.Core;

public sealed class ContractPeriod
{
    [JsonPropertyName("BEGIN")]
    public string BeginText { get; set; } = string.Empty;

    [JsonPropertyName("END")]
    public string? EndText { get; set; }

    [JsonIgnore]
    public DateTimeOffset Begin => DateTimeOffset.Parse(BeginText);

    [JsonIgnore]
    public DateTimeOffset? End =>
        string.IsNullOrWhiteSpace(EndText) ? null : DateTimeOffset.Parse(EndText);
}
