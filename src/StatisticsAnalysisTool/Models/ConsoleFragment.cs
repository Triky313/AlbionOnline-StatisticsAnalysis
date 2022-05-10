using System;
using System.Collections.Generic;
using System.Text.Json;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models;

public class ConsoleFragment
{
    public ConsoleFragment(string eventName, string text, ConsoleColorType consoleColorType)
    {
        Timestamp = DateTime.UtcNow;
        EventName = eventName;
        Text = text;
        ConsoleColorType = consoleColorType;
    }

    public ConsoleFragment(string eventName, Dictionary<byte, object> parameters, ConsoleColorType consoleColorType)
    {
        Timestamp = DateTime.UtcNow;
        EventName = eventName;
        Text = JsonSerializer.Serialize(parameters);
        ConsoleColorType = consoleColorType;
    }

    public DateTime Timestamp { get; set; }
    public string EventName { get; set; }
    public string Text { get; set; }
    public ConsoleColorType ConsoleColorType { get; set; }
    public string Output => $"[{Timestamp}] {EventName}: {Text}";
}