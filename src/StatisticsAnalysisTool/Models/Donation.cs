using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace StatisticsAnalysisTool.Models;

public class Donation
{
    public DateTime Timestamp { get; set; }
    public long Amount { get; set; }
    public double RealMoneyAmount { get; set; }
    public string Contributor { get; set; }
    public bool IsDonationRealMoney { get; set; } = false;
    public string Server { get; set; } = "US";

    [JsonIgnore] 
    public Visibility DonationRealMoneyVisibility => (IsDonationRealMoney) ? Visibility.Visible : Visibility.Collapsed;
    [JsonIgnore] 
    public Visibility DonationSilverVisibility => (IsDonationRealMoney) ? Visibility.Collapsed : Visibility.Visible;

    [JsonIgnore]
    public string DonationServer
    {
        get
        {
            if (string.IsNullOrEmpty(Server))
            {
                return LanguageController.Translation("AMERICA_SERVER");
            }

            return Server switch
            {
                "EU" => LanguageController.Translation("EUROPE_SERVER"),
                "US" => LanguageController.Translation("AMERICA_SERVER"),
                "ASIA" => LanguageController.Translation("ASIA_SERVER"),
                _ => LanguageController.Translation("AMERICA_SERVER")
            };
        }
    }

    [JsonIgnore]
    public DonationType DonationType
    {
        get
        {
            if (!IsDonationRealMoney)
            {
                return Amount switch
                {
                    <= 50000 => DonationType.Toad,
                    <= 75000 => DonationType.Marmot,
                    <= 100000 => DonationType.HillMarmot,
                    <= 250000 => DonationType.Rabbit,
                    <= 500000 => DonationType.SnowRabbit,
                    <= 750000 => DonationType.Snake,
                    <= 1000000 => DonationType.Snake,
                    <= 1500000 => DonationType.Fox,
                    <= 2000000 => DonationType.Impala,
                    <= 2500000 => DonationType.MoaBird,
                    <= 3000000 => DonationType.Wolf,
                    <= 3500000 => DonationType.CritterCougar,
                    <= 4000000 => DonationType.GiantToad,
                    <= 4500000 => DonationType.Boar,
                    <= 5000000 => DonationType.GiantStag,
                    <= 5500000 => DonationType.MonitorLizard,
                    <= 6000000 => DonationType.CritterMistCougar,
                    <= 7500000 => DonationType.Bear,
                    <= 8000000 => DonationType.GiantSnake,
                    <= 9000000 => DonationType.Marabou,
                    <= 10000000 => DonationType.CritterMistCougar2,
                    <= 12500000 => DonationType.DireWolf,
                    <= 15000000 => DonationType.GiantStagMoose,
                    <= 17500000 => DonationType.Alligator,
                    <= 20000000 => DonationType.DesertWolf,
                    <= 22500000 => DonationType.CritterMistCougar3,
                    <= 25000000 => DonationType.DireBoar2,
                    <= 37500000 => DonationType.Rhino,
                    <= 50000000 => DonationType.Rhino2,
                    <= 75000000 => DonationType.DireBoar,
                    <= 100000000 => DonationType.CritterMistCougar4,
                    <= 250000000 => DonationType.DireBear2,
                    <= 500000000 => DonationType.DireBear,
                    <= 750000000 => DonationType.CritterMistCougar5,
                    <= 1000000000 => DonationType.Mammoth,
                    <= 10000000000 => DonationType.AncientMammoth,
                    _ => DonationType.Toad
                };
            }

            return RealMoneyAmount switch
            {
                <= 1 => DonationType.Toad,
                <= 2 => DonationType.Marmot,
                <= 3 => DonationType.HillMarmot,
                <= 4 => DonationType.Rabbit,
                <= 5 => DonationType.SnowRabbit,
                <= 10 => DonationType.Snake,
                <= 20 => DonationType.Snake,
                <= 30 => DonationType.Fox,
                <= 40 => DonationType.Impala,
                <= 50 => DonationType.MoaBird,
                <= 60 => DonationType.Wolf,
                <= 70 => DonationType.CritterCougar,
                <= 80 => DonationType.GiantToad,
                <= 90 => DonationType.Boar,
                <= 100 => DonationType.GiantStag,
                <= 125 => DonationType.MonitorLizard,
                <= 250 => DonationType.CritterMistCougar,
                <= 375 => DonationType.Bear,
                <= 500 => DonationType.GiantSnake,
                <= 750 => DonationType.Marabou,
                <= 825 => DonationType.CritterMistCougar2,
                <= 1000 => DonationType.DireWolf,
                <= 1250 => DonationType.GiantStagMoose,
                <= 1375 => DonationType.Alligator,
                <= 1500 => DonationType.DesertWolf,
                <= 1750 => DonationType.CritterMistCougar3,
                <= 2000 => DonationType.DireBoar2,
                <= 3000 => DonationType.Rhino,
                <= 4000 => DonationType.Rhino2,
                <= 5000 => DonationType.DireBoar,
                <= 6000 => DonationType.CritterMistCougar4,
                <= 7000 => DonationType.DireBear2,
                <= 8000 => DonationType.DireBear,
                <= 9000 => DonationType.CritterMistCougar5,
                <= 10000 => DonationType.Mammoth,
                <= 31300 => DonationType.AncientMammoth,
                _ => DonationType.Toad
            };
        }
    }

    [JsonIgnore]
    public static string TranslationSilver => LanguageController.Translation("SILVER");
}