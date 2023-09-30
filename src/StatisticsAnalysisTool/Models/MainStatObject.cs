using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Models;

public class MainStatObject : BaseViewModel
{
    private CityFaction _cityFaction = CityFaction.Unknown;
    private double _value;
    private double _valuePerHour;
    private Visibility _visibility = Visibility.Hidden;

    public CityFaction CityFaction
    {
        get => _cityFaction;
        set
        {
            _cityFaction = value;
            Visibility = _cityFaction == CityFaction.Unknown ? Visibility.Hidden : Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public double Value
    {
        get => _value;
        set
        {
            _value = value;
            OnPropertyChanged();
        }
    }

    public double ValuePerHour
    {
        get => _valuePerHour;
        set
        {
            _valuePerHour = value;
            OnPropertyChanged();
        }
    }

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationTotalFactionPoints => LanguageController.Translation("TOTAL_FACTION_POINTS");
}