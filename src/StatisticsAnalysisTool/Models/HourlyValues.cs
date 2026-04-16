using System;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Models;

public class HourlyValues
{
    public HourlyValues()
    {
    }

    public HourlyValues(ValueType valueType, double value, DateTime date)
    {
        ValueType = valueType;
        Value = value;
        Date = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0);
    }

    public ValueType ValueType
    {
        get;
        set;
    }

    public double Value
    {
        get;
        set;
    }

    public DateTime Date
    {
        get;
        set;
    }

    public void Add(HourlyValues hourlyValues)
    {
        Value += hourlyValues.Value;
    }
}