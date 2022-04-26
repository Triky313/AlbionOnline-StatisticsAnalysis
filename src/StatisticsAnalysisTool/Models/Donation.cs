using System;
using System.Text.Json.Serialization;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models;

public class Donation
{
    public DateTime Timestamp { get; set; }
    public long Amount { get; set; }
    public string Contributor { get; set; }

    [JsonIgnore]
    public static string TranslationSilver => LanguageController.Translation("SILVER");
    [JsonIgnore]
    public DonationType DonationType {
        get
        {
            return Amount switch
            {
                < 100000 => DonationType.Normal,
                < 1000000 => DonationType.Good,
                < 10000000 => DonationType.Outstanding,
                < 100000000 => DonationType.Excellent,
                < 1000000000 => DonationType.Masterpeace,
                < 10000000000 => DonationType.Legendary,
                _ => DonationType.Normal
            };
        }
    }
}