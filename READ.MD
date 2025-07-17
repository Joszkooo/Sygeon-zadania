# Solution Methods

## Task 2 – Pancake Ingredient Hourly Report (Europe/Warsaw)

### What the method does

1. Reads a JSON array of ingredient rows.
2. Converts each timestamp (with its offset, e.g., `+09:00`) to **Europe/Warsaw** local time.
3. Rounds **down to the start of the hour** (minute/second = 00).
4. Groups rows by that hour.
5. Sums the amounts.
6. Converts units:

   * FLOUR: dkg → kg (divide by 100)
   * GROAT: g   → kg (divide by 1000)
   * MILK:  ml  → L  (divide by 1000)
   * EGG:   pieces (unchanged)
7. Rounds to **2 decimal places** (AwayFromZero).
8. Writes aggregated JSON.

### Core data types

```csharp
public sealed class PancakeUsageRow
{
    public DateTimeOffset Timestamp { get; init; } // input TIMESTAMP
    public decimal FLOUR { get; init; } // dkg
    public decimal GROAT { get; init; } // g
    public decimal MILK  { get; init; } // ml
    public decimal EGG   { get; init; } // pieces
}

public sealed class PancakeAggRow
{
    public DateTimeOffset Timestamp { get; init; } // Warsaw start-of-hour
    public decimal FLOUR_KG { get; init; }
    public decimal GROAT_KG { get; init; }
    public decimal MILK_L   { get; init; }
    public decimal EGG_PCS  { get; init; }
}
```

### Core method

```csharp
public static IReadOnlyList<PancakeAggRow> AggregateByHour(
    IEnumerable<PancakeUsageRow> rows,
    TimeZoneInfo warsawTz)
{
    static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    var grouped = new Dictionary<DateTimeOffset,(decimal flour,decimal groat,decimal milk,decimal egg)>();

    foreach (var r in rows)
    {
        // Convert to Warsaw
        var local = TimeZoneInfo.ConvertTime(r.Timestamp, warsawTz);
        // Floor to hour
        var floored = new DateTimeOffset(local.Year, local.Month, local.Day, local.Hour, 0, 0, local.Offset);

        if (!grouped.TryGetValue(floored, out var agg))
            agg = (0,0,0,0);

        agg.flour += r.FLOUR; // still in dkg
        agg.groat += r.GROAT; // g
        agg.milk  += r.MILK;  // ml
        agg.egg   += r.EGG;   // pcs
        grouped[floored] = agg;
    }

    // Build result (sorted by timestamp)
    var list = new List<PancakeAggRow>(grouped.Count);
    foreach (var kvp in grouped)
    {
        var ts = kvp.Key;
        var (f,g,m,e) = kvp.Value;
        list.Add(new PancakeAggRow
        {
            Timestamp = ts,
            FLOUR_KG = R2(f / 100m),    // dkg→kg
            GROAT_KG = R2(g / 1000m),   // g→kg
            MILK_L   = R2(m / 1000m),   // ml→L
            EGG_PCS  = R2(e)
        });
    }

    list.Sort((a,b) => a.Timestamp.CompareTo(b.Timestamp));
    return list;
}
```

### Input (`data.json`)

```json
[
  {"TIMESTAMP": "2023-04-13 00:38:00+09:00", "FLOUR": 170, "GROAT": 90, "MILK": 2200, "EGG": 90},
  {"TIMESTAMP": "2023-04-12 22:44:00+09:00", "FLOUR": 160, "GROAT": 60, "MILK": 2850, "EGG": 35},
  {"TIMESTAMP": "2023-04-12 00:30:00+09:00", "FLOUR": 100, "GROAT": 160, "MILK": 2300, "EGG": 46},
  {"TIMESTAMP": "2023-04-13 00:24:00+09:00", "FLOUR": 140, "GROAT": 10, "MILK": 2800, "EGG": 95},
  {"TIMESTAMP": "2023-04-13 00:39:00+09:00", "FLOUR": 130, "GROAT": 130, "MILK": 2400, "EGG": 78},
  {"TIMESTAMP": "2023-04-13 00:25:00+09:00", "FLOUR": 180, "GROAT": 80, "MILK": 2050, "EGG": 88},
  {"TIMESTAMP": "2023-04-11 22:39:00+09:00", "FLOUR": 120, "GROAT": 110, "MILK": 2900, "EGG": 32},
  {"TIMESTAMP": "2023-04-12 22:23:00+09:00", "FLOUR": 150, "GROAT": 100, "MILK": 2000, "EGG": 23},
  {"TIMESTAMP": "2023-04-11 23:52:00+09:00", "FLOUR": 190, "GROAT": 50, "MILK": 2650, "EGG": 79},
  {"TIMESTAMP": "2023-04-13 00:55:00+09:00", "FLOUR": 110, "GROAT": 30, "MILK": 2550, "EGG": 99}
]
```

### Call

```bash
dotnet run --project .\task2Sygeon.Cli\ -- .\data.json output.json
```

### Sample result JSON (Warsaw start-of-hour)

```json
[
  {
    "TIMESTAMP": "2023-04-11 15:00:00 \u002B02:00",
    "FLOUR_KG": 1.2,
    "GROAT_KG": 0.11,
    "MILK_L": 2.9,
    "EGG_PCS": 32
  },
  {
    "TIMESTAMP": "2023-04-11 16:00:00 \u002B02:00",
    "FLOUR_KG": 1.9,
    "GROAT_KG": 0.05,
    "MILK_L": 2.65,
    "EGG_PCS": 79
  },
  {
    "TIMESTAMP": "2023-04-11 17:00:00 \u002B02:00",
    "FLOUR_KG": 1,
    "GROAT_KG": 0.16,
    "MILK_L": 2.3,
    "EGG_PCS": 46
  },
  {
    "TIMESTAMP": "2023-04-12 15:00:00 \u002B02:00",
    "FLOUR_KG": 3.1,
    "GROAT_KG": 0.16,
    "MILK_L": 4.85,
    "EGG_PCS": 58
  },
  {
    "TIMESTAMP": "2023-04-12 17:00:00 \u002B02:00",
    "FLOUR_KG": 7.3,
    "GROAT_KG": 0.34,
    "MILK_L": 12,
    "EGG_PCS": 450
  }
]
```

---

## Task 2 – Example command

You can call your CLI like this (example pathing on Windows PowerShell):

```powershell
dotnet run --project .\task2Sygeon.Cli\ -- .\data.json output.json
```

* First arg: input JSON path.
* Second arg: output path (aggregated JSON).

---

## Task 3 – HR Reporting Intervals (Europe/Warsaw)

### What the method does

Given a **report window** (begin/end) and a list of **contract periods** (each begin + optional end; `null` = open), produce the **sorted list of sub‑intervals** inside the window where contract coverage can change. Include gaps (no contract).

### Minimal data types

```csharp
public sealed class ContractPeriod
{
    public DateTimeOffset Begin { get; init; }
    public DateTimeOffset? End { get; init; } // null = open-ended
}

public sealed class ReportInterval
{
    public DateTimeOffset Begin { get; init; }
    public DateTimeOffset End   { get; init; }
}
```

### Core method

```csharp
public static List<ReportInterval> BuildIntervals(
    DateTimeOffset reportBegin,
    DateTimeOffset reportEnd,
    IEnumerable<ContractPeriod> contracts,
    TimeZoneInfo warsawTz)
{
    if (reportEnd <= reportBegin) throw new ArgumentException("Report end must be after start");

    // Convert report bounds to Warsaw
    var rb = TimeZoneInfo.ConvertTime(reportBegin, warsawTz);
    var re = TimeZoneInfo.ConvertTime(reportEnd,   warsawTz);

    var boundaries = new HashSet<DateTimeOffset> { rb, re };

    foreach (var c in contracts)
    {
        var cb = TimeZoneInfo.ConvertTime(c.Begin, warsawTz);
        var ce = c.End.HasValue ? TimeZoneInfo.ConvertTime(c.End.Value, warsawTz) : re; // open→report end

        if (ce <= rb) continue; // ends before window
        if (cb >= re) continue; // starts after window

        if (cb < rb) cb = rb;   // clip
        if (ce > re) ce = re;

        boundaries.Add(cb);
        boundaries.Add(ce);
    }

    var list = new List<DateTimeOffset>(boundaries);
    list.Sort();

    var result = new List<ReportInterval>(list.Count - 1);
    for (int i = 0; i < list.Count - 1; i++)
    {
        var b = list[i];
        var e = list[i + 1];
        if (e > b)
            result.Add(new ReportInterval { Begin = b, End = e });
    }
    return result;
}
```

### Interval formatting helper

```csharp
static string Iso(DateTimeOffset dt) => dt.ToString("yyyy-MM-dd'T'HH:mm:sszzz");
```

### Contracts JSON

```json
[
  { "BEGIN": "2022-06-05T10:00:00+12:00", "END": "2022-07-03T18:00:00-04:00" },
  { "BEGIN": "2022-07-29T12:00:00-10:00", "END": "2023-03-28T14:00:00-08:00" },
  { "BEGIN": "2023-04-02T06:00:00+08:00", "END": null },
  { "BEGIN": "2023-03-28T14:00:00-08:00", "END": "2023-04-02T06:00:00+08:00" },
  { "BEGIN": "2022-07-06T11:00:00+13:00", "END": "2022-07-19T12:00:00-10:00" },
  { "BEGIN": "2022-03-15T19:00:00-04:00", "END": "2022-05-13T00:00:00+02:00" }
]
```

### Call

```bash
dotnet run --project HRIntervals.Cli -- data.json 2022-06-15T00:00:00+02:00 2023-04-30T00:00:00+02:00 --out intervals.json
```

### Result JSON

```json
[
  [
    "2022-06-15T00:00:00\u002B02:00",
    "2022-07-04T00:00:00\u002B02:00"
  ],
  [
    "2022-07-04T00:00:00\u002B02:00",
    "2022-07-06T00:00:00\u002B02:00"
  ],
  [
    "2022-07-06T00:00:00\u002B02:00",
    "2022-07-20T00:00:00\u002B02:00"
  ],
  [
    "2022-07-20T00:00:00\u002B02:00",
    "2022-07-30T00:00:00\u002B02:00"
  ],
  [
    "2022-07-30T00:00:00\u002B02:00",
    "2023-03-29T00:00:00\u002B02:00"
  ],
  [
    "2023-03-29T00:00:00\u002B02:00",
    "2023-04-02T00:00:00\u002B02:00"
  ],
  [
    "2023-04-02T00:00:00\u002B02:00",
    "2023-04-30T00:00:00\u002B02:00"
  ]
]
```
