namespace task2Sygeon.Core;

public class UsageRecord
{
  public string TIMESTAMP { get; set; } = string.Empty; // 2023-04-13 00:38:00+09:00
  public decimal FLOUR { get; set; }  // dkg
  public decimal GROAT { get; set; }  // g
  public decimal MILK  { get; set; }  // ml
  public decimal EGG   { get; set; }  // pc
}

public class AggregatedRow
{
  public string TIMESTAMP { get; set; } = string.Empty; // YYYY-mm-dd HH:MM:SS Z
  public decimal FLOUR_KG { get; set; } // kg
  public decimal GROAT_KG { get; set; } // kg
  public decimal MILK_L   { get; set; } // l
  public decimal EGG_PCS  { get; set; } // pc
}