using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Dungeon.Models;

public sealed class DungeonMetric : BaseViewModel
{
    private readonly bool _isLowerValueBetter;
    private double _difference;

    public DungeonMetric(bool hasPerHourValue = false, bool isLowerValueBetter = false)
    {
        HasPerHourValue = hasPerHourValue;
        _isLowerValueBetter = isLowerValueBetter;
    }

    public double Value
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double ValuePerHour
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double ChangeAmount
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double ChangePercentage
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool HasPerHourValue { get; }
    public bool IsIncrease => _difference > 0;
    public bool IsDecrease => _difference < 0;
    public bool IsPositiveChange => _isLowerValueBetter ? IsDecrease : IsIncrease;
    public bool IsNegativeChange => _isLowerValueBetter ? IsIncrease : IsDecrease;
    public string ChangeIndicator => IsIncrease ? "▲" : IsDecrease ? "▼" : "—";
    public string ChangePercentageText => $"{ChangePercentage:+0.00;-0.00;0.00}%";

    public void Update(double value, double previousPeriodValue, double valuePerHour = 0)
    {
        Value = value;
        ValuePerHour = valuePerHour;
        _difference = value - previousPeriodValue;
        ChangeAmount = Math.Abs(_difference);
        ChangePercentage = CalculateChangePercentage(value, previousPeriodValue);
        OnPropertyChanged(nameof(IsIncrease));
        OnPropertyChanged(nameof(IsDecrease));
        OnPropertyChanged(nameof(IsPositiveChange));
        OnPropertyChanged(nameof(IsNegativeChange));
        OnPropertyChanged(nameof(ChangeIndicator));
        OnPropertyChanged(nameof(ChangePercentageText));
    }

    private static double CalculateChangePercentage(double currentValue, double previousValue)
    {
        if (previousValue == 0)
        {
            return currentValue switch
            {
                > 0 => 100,
                < 0 => -100,
                _ => 0
            };
        }

        return (currentValue - previousValue) / Math.Abs(previousValue) * 100;
    }
}