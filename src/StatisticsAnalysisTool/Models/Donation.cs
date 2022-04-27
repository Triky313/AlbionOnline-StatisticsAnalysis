using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
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

    [JsonIgnore] 
    public Visibility DonationRealMoneyVisibility => (IsDonationRealMoney) ? Visibility.Visible : Visibility.Collapsed;
    [JsonIgnore] 
    public Visibility DonationSilverVisibility => (IsDonationRealMoney) ? Visibility.Collapsed : Visibility.Visible;
    [JsonIgnore]
    public DonationType DonationType
    {
        get
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
    }

    [JsonIgnore]
    public static string TranslationSilver => LanguageController.Translation("SILVER");
}