using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardSummaryMetric : BaseViewModel
{
    private double _difference;

    public double Value
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsValueNegative));
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

    public bool IsIncrease => _difference > 0;
    public bool IsDecrease => _difference < 0;
    public bool IsValueNegative => Value < 0;
    public string ChangeIndicator => IsIncrease ? "\u25B2" : IsDecrease ? "\u25BC" : "\u2014";
    public string ChangePercentageText => $"{ChangePercentage:+0.00;-0.00;0.00}%";

    public void Update(double value, double currentPeriodValue, double previousPeriodValue)
    {
        Value = value;
        _difference = currentPeriodValue - previousPeriodValue;
        ChangeAmount = Math.Abs(_difference);
        ChangePercentage = CalculateChangePercentage(currentPeriodValue, previousPeriodValue);
        OnPropertyChanged(nameof(IsIncrease));
        OnPropertyChanged(nameof(IsDecrease));
        OnPropertyChanged(nameof(ChangeIndicator));
        OnPropertyChanged(nameof(ChangePercentageText));
    }

    public void UpdateValuePerHour(double valuePerHour)
    {
        ValuePerHour = valuePerHour;
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