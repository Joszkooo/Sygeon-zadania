using System.Text;
using task2Sygeon.Core;

string? inputPath = args.Length > 0 ? args[0] : null;
string? outputPath = args.Length > 1 ? args[1] : null;

if (string.IsNullOrWhiteSpace(inputPath))
{
    Console.WriteLine("Usage: PancakeReport <inputPath|-> [outputPath|-]");
    return 1;
}

string json;
if (inputPath == "-")
{
    using var sr = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
    json = sr.ReadToEnd();
}
else
{
    if (!File.Exists(inputPath))
    {
        Console.Error.WriteLine($"Input file not found: {inputPath}");
        return 2;
    }
    json = File.ReadAllText(inputPath, Encoding.UTF8);
}

var aggRows = PancakeAggregator.AggregateJson(json);
var outJson = PancakeAggregator.ToJson(aggRows, indented: true);

if (string.IsNullOrWhiteSpace(outputPath) || outputPath == "-")
{
    Console.OutputEncoding = Encoding.UTF8;
    Console.WriteLine(outJson);
}
else
{
    File.WriteAllText(outputPath, outJson, Encoding.UTF8);
}

return 0;