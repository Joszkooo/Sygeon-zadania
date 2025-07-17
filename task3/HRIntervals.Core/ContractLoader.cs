using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace HRIntervals.Core;

public static class ContractLoader
{
    static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static IReadOnlyList<ContractPeriod> LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<List<ContractPeriod>>(json, Options);
        return data ?? new List<ContractPeriod>();
    }
}
