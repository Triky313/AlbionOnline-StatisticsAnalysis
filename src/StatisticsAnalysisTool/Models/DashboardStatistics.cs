using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;

public class DashboardStatistics
{
    public List<DailyValues> DailyValues { get; set; } = new();

    [JsonIgnore]
    private bool _wasExecuted;

    public void Add(DailyValues dailyValues)
    {
        var dailyValue = DailyValues.FirstOrDefault(x => x.Date.Date == dailyValues.Date.Date && x.ValueType == dailyValues.ValueType);

        if (dailyValue == null)
        {
            DailyValues.Add(dailyValues);
        }
        else
        {
            lock (dailyValue)
            {
                dailyValue.Add(dailyValues);
            }
        }

        RemoveData(DateTime.UtcNow.Date.AddDays(Settings.Default.KeepDashboardStatisticsForDays));
    }

    private void RemoveData(DateTime afterDateDataWillBeDeleted)
    {
        if (_wasExecuted)
        {
            return;
        }

        DailyValues.RemoveAll(x => x.Date.Date < afterDateDataWillBeDeleted.Date);

        _wasExecuted = true;
    }
}