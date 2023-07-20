using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.PartyBuilder;

public class PartyBuilderBindings : INotifyPropertyChanged
{
    private ObservableCollection<PartyBuilderPlayer> _party = new();
    private ListCollectionView _partyCollectionView;
    private GridLength _gridSplitterPosition;
    private double _minimalItemPower;
    private double _maximumItemPower;
    private double _minimalBasicItemPower;
    private double _maximumBasicItemPower;
    private double _averagePartyIp;
    private double _averagePartyBasicIp;

    public PartyBuilderBindings()
    {
        PartyCollectionView = CollectionViewSource.GetDefaultView(Party) as ListCollectionView;

        if (PartyCollectionView != null)
        {
            PartyCollectionView.IsLiveSorting = true;
            PartyCollectionView.IsLiveFiltering = true;
            PartyCollectionView.CustomSort = new PartyPlannerPlayerComparer();
            PartyCollectionView.Refresh();
        }

        MinimalItemPower = SettingsController.CurrentSettings.PartyBuilderMinimalItemPower;
        MaximumItemPower = SettingsController.CurrentSettings.PartyBuilderMaximumItemPower;
        MinimalBasicItemPower = SettingsController.CurrentSettings.PartyBuilderMinimalBasicItemPower;
        MaximumBasicItemPower = SettingsController.CurrentSettings.PartyBuilderMaximumBasicItemPower;

        Party.CollectionChanged += UpdateAveragePartyIp;
    }

    public ListCollectionView PartyCollectionView
    {
        get => _partyCollectionView;
        set
        {
            _partyCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PartyBuilderPlayer> Party
    {
        get => _party;
        set
        {
            _party = value;
            OnPropertyChanged();
        }
    }


    public double MinimalItemPower
    {
        get => _minimalItemPower;
        set
        {
            _minimalItemPower = value;
            SettingsController.CurrentSettings.PartyBuilderMinimalItemPower = _minimalItemPower;
            UpdatePartyBuilderPlayerConditionsMinIP(value);
            OnPropertyChanged();
        }
    }

    public double MaximumItemPower
    {
        get => _maximumItemPower;
        set
        {
            _maximumItemPower = value;
            SettingsController.CurrentSettings.PartyBuilderMaximumItemPower = _maximumItemPower;
            UpdatePartyBuilderPlayerConditionsMaxIP(value);
            OnPropertyChanged();
        }
    }

    public double MinimalBasicItemPower
    {
        get => _minimalBasicItemPower;
        set
        {
            _minimalBasicItemPower = value;
            SettingsController.CurrentSettings.PartyBuilderMinimalBasicItemPower = _minimalBasicItemPower;
            UpdatePartyBuilderPlayerConditionsMinBIP(value);
            OnPropertyChanged();
        }
    }

    public double MaximumBasicItemPower
    {
        get => _maximumBasicItemPower;
        set
        {
            _maximumBasicItemPower = value;
            SettingsController.CurrentSettings.PartyBuilderMaximumBasicItemPower = _maximumBasicItemPower;
            UpdatePartyBuilderPlayerConditionsMaxBIP(value);
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.PartyBuilderGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    public double AveragePartyIp
    {
        get => _averagePartyIp;
        set
        {
            _averagePartyIp = value;
            OnPropertyChanged();
        }
    }

    public double AveragePartyBasicIp
    {
        get => _averagePartyBasicIp;
        set
        {
            _averagePartyBasicIp = value;
            OnPropertyChanged();
        }
    }

    public void UpdatePartyBuilderPlayerConditions(double minimalBasicItemPower, double maximumBasicItemPower, double minimalItemPower, double maximumItemPower)
    {
        foreach (PartyBuilderPlayer partyBuilderPlayer in Party)
        {
            GetItemPowerCondition(partyBuilderPlayer, minimalBasicItemPower, maximumBasicItemPower, minimalItemPower, maximumItemPower);
        }
    }

    public void UpdatePartyBuilderPlayerConditionsMinBIP(double minimalBasicItemPower)
    {
        foreach (PartyBuilderPlayer partyBuilderPlayer in Party)
        {
            GetItemPowerCondition(partyBuilderPlayer, minimalBasicItemPower, MaximumBasicItemPower, MinimalItemPower, MaximumItemPower);
        }
    }

    public void UpdatePartyBuilderPlayerConditionsMaxBIP(double maximumBasicItemPower)
    {
        foreach (PartyBuilderPlayer partyBuilderPlayer in Party)
        {
            GetItemPowerCondition(partyBuilderPlayer, MinimalBasicItemPower, maximumBasicItemPower, MinimalItemPower, MaximumItemPower);
        }
    }

    public void UpdatePartyBuilderPlayerConditionsMinIP(double minimalItemPower)
    {
        foreach (PartyBuilderPlayer partyBuilderPlayer in Party)
        {
            GetItemPowerCondition(partyBuilderPlayer, MinimalBasicItemPower, MaximumBasicItemPower, minimalItemPower, MaximumItemPower);
        }
    }

    public void UpdatePartyBuilderPlayerConditionsMaxIP(double maximumItemPower)
    {
        foreach (PartyBuilderPlayer partyBuilderPlayer in Party)
        {
            GetItemPowerCondition(partyBuilderPlayer, MinimalBasicItemPower, MaximumBasicItemPower, MinimalItemPower, maximumItemPower);
        }
    }

    public void UpdatePartyBuilderPlayerConditions()
    {
        foreach (PartyBuilderPlayer partyBuilderPlayer in Party)
        {
            GetItemPowerCondition(partyBuilderPlayer, MinimalBasicItemPower, MaximumBasicItemPower, MinimalItemPower, MaximumItemPower);
        }
    }

    private void GetItemPowerCondition(
        PartyBuilderPlayer partyBuilderPlayer,
        double minimalBasicItemPower,
        double maximumBasicItemPower,
        double minimalItemPower,
        double maximumItemPower)
    {
        if (partyBuilderPlayer.AverageBasicItemPower.ItemPower < minimalBasicItemPower)
        {
            partyBuilderPlayer.BasicItemPowerCondition = PartyBuilderItemPowerCondition.UnderMinimal;
        }
        else if (partyBuilderPlayer.AverageBasicItemPower.ItemPower > maximumBasicItemPower)
        {
            partyBuilderPlayer.BasicItemPowerCondition = PartyBuilderItemPowerCondition.AboveMaximum;
        }
        else if (partyBuilderPlayer.AverageBasicItemPower.ItemPower >= minimalBasicItemPower && partyBuilderPlayer.AverageBasicItemPower.ItemPower <= maximumBasicItemPower)
        {
            partyBuilderPlayer.BasicItemPowerCondition = PartyBuilderItemPowerCondition.Normal;
        }
        else
        {
            partyBuilderPlayer.BasicItemPowerCondition = PartyBuilderItemPowerCondition.Unknown;
        }

        if (partyBuilderPlayer.AverageItemPower.ItemPower < minimalItemPower)
        {
            partyBuilderPlayer.ItemPowerCondition = PartyBuilderItemPowerCondition.UnderMinimal;
        }
        else if (partyBuilderPlayer.AverageItemPower.ItemPower > maximumItemPower)
        {
            partyBuilderPlayer.ItemPowerCondition = PartyBuilderItemPowerCondition.AboveMaximum;
        }
        else if (partyBuilderPlayer.AverageItemPower.ItemPower >= minimalItemPower && partyBuilderPlayer.AverageItemPower.ItemPower <= maximumItemPower)
        {
            partyBuilderPlayer.ItemPowerCondition = PartyBuilderItemPowerCondition.Normal;
        }
        else
        {
            partyBuilderPlayer.ItemPowerCondition = PartyBuilderItemPowerCondition.Unknown;
        }
    }

    public void UpdateAveragePartyIp(object sender, NotifyCollectionChangedEventArgs e)
    {
        var ipSum = Party.Where(x => x.IsLocalPlayer == false).Sum(x => x.AverageItemPower.ItemPower);
        var basicIpSum = Party.Sum(x => x.AverageBasicItemPower.ItemPower);
        if (Party.Count <= 0)
        {
            return;
        }

        if (ipSum > 0)
        {
            AveragePartyIp = ipSum / Party.Count(x => x.IsLocalPlayer == false);
        }
        else
        {
            AveragePartyIp = 0;
        }

        if (basicIpSum > 0)
        {
            AveragePartyBasicIp = basicIpSum / Party.Count;
        }
        else
        {
            AveragePartyBasicIp = 0;
        }
    }

    public static string TranslationDescriptions => LanguageController.Translation("DESCRIPTIONS");
    public static string TranslationPlayerIsNotInspected => LanguageController.Translation("PLAYER_IS_NOT_INSPECTED");
    public static string TranslationItemPower => LanguageController.Translation("ITEM_POWER");
    public static string TranslationBasicItemPower => LanguageController.Translation("BASIC_ITEM_POWER");
    public static string TranslationConditions => LanguageController.Translation("CONDITIONS");
    public static string TranslationMinimalItemPower => LanguageController.Translation("MINIMAL_ITEM_POWER");
    public static string TranslationMinimalBasicItemPower => LanguageController.Translation("MINIMAL_BASIC_ITEM_POWER");
    public static string TranslationMaximumItemPower => LanguageController.Translation("MAXIMUM_ITEM_POWER");
    public static string TranslationMaximumBasicItemPower => LanguageController.Translation("MAXIMUM_BASIC_ITEM_POWER");
    public static string TranslationIpOrBipBetweenMinAndMaxRange => LanguageController.Translation("IP_OR_BIP_BETWEEN_MIN_AND_MAX_RANGE");
    public static string TranslationIpOrBipOverMaxRange => LanguageController.Translation("IP_OR_BIP_OVER_MAX_RANGE");
    public static string TranslationIpOrBipUnderMinRange => LanguageController.Translation("IP_OR_BIP_UNDER_MIN_RANGE");
    public static string TranslationPartyInformation => LanguageController.Translation("PARTY_INFORMATION");
    public static string TranslationAverageIp => LanguageController.Translation("AVERAGE_IP");
    public static string TranslationAverageBasicIp => LanguageController.Translation("AVERAGE_BIP");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}