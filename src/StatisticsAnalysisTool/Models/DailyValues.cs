using System;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Models;

public class DailyValues
{
    public DailyValues(ValueType valueType, double value, DateTime date)
    {
        ValueType = valueType;
        Value = value;
        Date = date.Date;
    }

    public ValueType ValueType { get; set; }
    public double Value { get; set; }
    public DateTime Date { get; set; }

    public void Add(DailyValues dailyValues)
    {
        Value += dailyValues.Value;
    }
}