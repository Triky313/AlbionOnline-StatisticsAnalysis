using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models.ItemWindowModel;

public class RequiredJournal(ItemWindowViewModel itemWindowViewModel) : BaseViewModel
{
    private BitmapImage _icon;
    private string _craftingResourceName;
    private long _costsPerJournal;
    private double _requiredJournalAmount;
    private double _sellPricePerJournal;
    private List<MarketResponse> _marketResponseEmptyJournal = new();
    private List<MarketResponse> _marketResponseFullJournal = new();
    private DateTime _lastUpdateEmptyJournal = DateTime.UtcNow.AddDays(-100);
    private DateTime _lastUpdateFullJournal = DateTime.UtcNow.AddDays(-100);
    private MarketLocation _itemPricesLocationEmptyJournalSelected;
    private MarketLocation _itemPricesLocationFullJournalSelected;
    private double _weight;
    private double _totalWeight;

    private static readonly KeyValuePair<MarketLocation, string>[] ItemPricesLocations = Locations.OnceMarketLocations;

    public string UniqueName { get; set; }

    private async void LoadSellPriceEmptyJournalAsync(MarketLocation location)
    {
        if (_lastUpdateEmptyJournal.AddMilliseconds(SettingsController.CurrentSettings.RefreshRate) < DateTime.UtcNow)
        {
            _marketResponseEmptyJournal = await ApiController.GetCityItemPricesFromJsonAsync(UniqueName);
            _lastUpdateEmptyJournal = DateTime.UtcNow;
        }

        var sellPriceMin = _marketResponseEmptyJournal?.OrderByDescending(x => x.SellPriceMinDate).ThenByDescending(x => x.SellPriceMin).FirstOrDefault(x => x.MarketLocation == location)?.SellPriceMin;
        if (sellPriceMin != null)
        {
            CostsPerJournal = (long) sellPriceMin;
        }
    }

    private async void LoadSellPriceFullJournalAsync(MarketLocation location)
    {
        var uniqueNameFullJournal = UniqueName.Replace("EMPTY", "FULL");
        if (_lastUpdateFullJournal.AddMilliseconds(SettingsController.CurrentSettings.RefreshRate) < DateTime.UtcNow)
        {
            _marketResponseFullJournal = await ApiController.GetCityItemPricesFromJsonAsync(uniqueNameFullJournal);
            _lastUpdateFullJournal = DateTime.UtcNow;
        }

        var sellPriceMin = _marketResponseFullJournal?.FirstOrDefault(x => string.Equals(x?.City, Locations.GetParameterName(location), StringComparison.CurrentCultureIgnoreCase))?.SellPriceMin;
        if (sellPriceMin != null)
        {
            SellPricePerJournal = (long) sellPriceMin;
        }
    }

    public KeyValuePair<MarketLocation, string>[] ItemPricesLocationsEmptyJournal => ItemPricesLocations;
    public KeyValuePair<MarketLocation, string>[] ItemPricesLocationsFullJournal => ItemPricesLocations;

    public MarketLocation ItemPricesLocationEmptyJournalSelected
    {
        get => _itemPricesLocationEmptyJournalSelected;
        set
        {
            _itemPricesLocationEmptyJournalSelected = value;
            LoadSellPriceEmptyJournalAsync(value);
            OnPropertyChanged();
        }
    }

    public MarketLocation ItemPricesLocationFullJournalSelected
    {
        get => _itemPricesLocationFullJournalSelected;
        set
        {
            _itemPricesLocationFullJournalSelected = value;
            LoadSellPriceFullJournalAsync(value);
            OnPropertyChanged();
        }
    }

    public BitmapImage Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public string CraftingResourceName
    {
        get => _craftingResourceName;
        set
        {
            _craftingResourceName = value;
            OnPropertyChanged();
        }
    }

    public long CostsPerJournal
    {
        get => _costsPerJournal;
        set
        {
            _costsPerJournal = value;
            itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public double RequiredJournalAmount
    {
        get => _requiredJournalAmount;
        set
        {
            _requiredJournalAmount = value;
            TotalWeight = Weight * value;
            OnPropertyChanged();
        }
    }

    public double SellPricePerJournal
    {
        get => _sellPricePerJournal;
        set
        {
            _sellPricePerJournal = value;
            itemWindowViewModel.UpdateCraftingCalculationTab();
            OnPropertyChanged();
        }
    }

    public double Weight
    {
        get => _weight;
        set
        {
            _weight = value;
            OnPropertyChanged();
        }
    }

    public double TotalWeight
    {
        get => _totalWeight;
        set
        {
            _totalWeight = value;
            OnPropertyChanged();
        }
    }

    #region Commands

    public void CopyItemNameToClipboard(object value)
    {
        Clipboard.SetDataObject(CraftingResourceName);
    }

    private ICommand _opyItemNameToClipboardCommand;

    public ICommand CopyItemNameToClipboardCommand => _opyItemNameToClipboardCommand ??= new CommandHandler(CopyItemNameToClipboard, true);

    #endregion

    public string TranslationRequiredJournals => LocalizationController.Translation("REQUIRED_JOURNALS");
    public string TranslationCostsPerJournal => LocalizationController.Translation("COSTS_PER_JOURNAL");
    public string TranslationRequiredJournalAmount => LocalizationController.Translation("REQUIRED_JOURNAL_AMOUNT");
    public string TranslationSellPricePerJournal => LocalizationController.Translation("SELL_PRICE_PER_JOURNAL");
    public string TranslationGetPrice => LocalizationController.Translation("GET_PRICE");
    public string TranslationTotalWeight => LocalizationController.Translation("TOTAL_WEIGHT");
}