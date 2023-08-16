using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Trade.Market;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Trade;

public class ManuallyTradeMenuObject : INotifyPropertyChanged
{
    private List<ManuallyTradeTypeStruct> _manuallyTradeTypes = new();
    private ManuallyTradeTypeStruct _manuallyTradeTypeSelection;
    private string _description;
    private long _value;

    public ManuallyTradeMenuObject()
    {
        ManuallyTradeTypes.Clear();
        ManuallyTradeTypes.Add(new ManuallyTradeTypeStruct() { Type = TradeType.ManualSell, Name = LanguageController.Translation("SOLD") });
        ManuallyTradeTypes.Add(new ManuallyTradeTypeStruct() { Type = TradeType.ManualBuy, Name = LanguageController.Translation("BOUGHT") });
    }

    public List<ManuallyTradeTypeStruct> ManuallyTradeTypes
    {
        get => _manuallyTradeTypes;
        set
        {
            _manuallyTradeTypes = value;
            OnPropertyChanged();
        }
    }

    public ManuallyTradeTypeStruct ManuallyTradeTypeSelection
    {
        get => _manuallyTradeTypeSelection;
        set
        {
            _manuallyTradeTypeSelection = value;
            OnPropertyChanged();
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    public long Value
    {
        get => _value;
        set
        {
            if (value > 9_000_000_000_000)
            {
                _value = 9_000_000_000_000;
            }
            _value = value;
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

    public static string TranslationDescription => LanguageController.Translation("DESCRIPTION");
    public static string TranslationAddTradeManually => LanguageController.Translation("ADD_TRADE_MANUALLY");
    public static string TranslationValue => LanguageController.Translation("VALUE");
    public static string TranslationAddTrade => LanguageController.Translation("ADD_TRADE");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}