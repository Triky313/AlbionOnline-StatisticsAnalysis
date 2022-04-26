using System;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Models;

public class Donation
{
    public DateTime Timestamp { get; set; }
    public long Amount { get; set; }
    public string Contributor { get; set; }

    [JsonIgnore]
    public static string TranslationSilver => LanguageController.Translation("SILVER");
}