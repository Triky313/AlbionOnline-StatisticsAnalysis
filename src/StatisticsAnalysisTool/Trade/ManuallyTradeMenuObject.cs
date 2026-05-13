using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Trade;

public class ManuallyTradeMenuObject : BaseViewModel
{
    public ManuallyTradeMenuObject()
    {
        RefreshManuallyTradeTypes();
    }

    public void RefreshLocalization()
    {
        RefreshManuallyTradeTypes();
        OnPropertyChanged(nameof(TranslationDescription));
        OnPropertyChanged(nameof(TranslationAddTradeManually));
        OnPropertyChanged(nameof(TranslationValue));
        OnPropertyChanged(nameof(TranslationAddTrade));
    }

    private void RefreshManuallyTradeTypes()
    {
        var selectedTradeType = ManuallyTradeTypeSelection.Type;
        var manuallyTradeTypes = new List<ManuallyTradeTypeStruct>
        {
            new() { Type = TradeType.ManualSell, Name = LocalizationController.Translation("SOLD") },
            new() { Type = TradeType.ManualBuy, Name = LocalizationController.Translation("BOUGHT") }
        };

        ManuallyTradeTypes = manuallyTradeTypes;
        ManuallyTradeTypeSelection = manuallyTradeTypes.FirstOrDefault(x => x.Type == selectedTradeType);
    }

    public List<ManuallyTradeTypeStruct> ManuallyTradeTypes
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ManuallyTradeTypeStruct ManuallyTradeTypeSelection
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public long Value
    {
        get;
        set
        {
            if (value > 9_000_000_000_000)
            {
                field = 9_000_000_000_000;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    #region Commands

    public void AddTrade(object obj)
    {
        if (Value == 0 || (ManuallyTradeTypeSelection.Type != TradeType.ManualSell && ManuallyTradeTypeSelection.Type != TradeType.ManualBuy))
        {
            return;
        }

        var dateTimeTicks = DateTime.UtcNow.Ticks;
        var trade = new Trade()
        {
            Ticks = dateTimeTicks,
            Type = ManuallyTradeTypeSelection.Type,
            Id = dateTimeTicks,
            ClusterIndex = default,
            AuctionEntry = default,
            Guid = Guid.NewGuid(),
            Description = Description,
            InstantBuySellContent = new InstantBuySellContent()
            {
                InternalUnitPrice = FixPoint.FromFloatingPointValue(Value).InternalValue,
                Quantity = 1,
                TaxRate = 0
            }
        };

        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.TradeController?.AddTradeToBindingCollectionAsync(trade);
    }

    private ICommand _addTradeCommand;

    public ICommand AddTradeCommand => _addTradeCommand ??= new CommandHandler(AddTrade, true);

    #endregion

    public static string TranslationDescription => LocalizationController.Translation("DESCRIPTION");
    public static string TranslationAddTradeManually => LocalizationController.Translation("ADD_TRADE_MANUALLY");
    public static string TranslationValue => LocalizationController.Translation("VALUE");
    public static string TranslationAddTrade => LocalizationController.Translation("ADD_TRADE");
}
