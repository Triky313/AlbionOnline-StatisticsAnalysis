using System;

namespace StatisticsAnalysisTool.Models;

public class Donation
{
    public DateTime Timestamp { get; set; }
    public long Amount { get; set; }
    public string Contributor { get; set; }
}