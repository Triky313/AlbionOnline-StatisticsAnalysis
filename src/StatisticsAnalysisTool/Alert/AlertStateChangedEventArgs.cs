using System;

namespace StatisticsAnalysisTool.Alert;

public sealed class AlertStateChangedEventArgs : EventArgs
{
    public AlertStateChangedEventArgs(string itemUniqueName)
    {
        ItemUniqueName = itemUniqueName;
    }

    public string ItemUniqueName { get; }
}