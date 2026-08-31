using System;

namespace StatisticsAnalysisTool.Common;

internal sealed class ItemImageStoredEventArgs : EventArgs
{
    public ItemImageStoredEventArgs(string itemUniqueName, int qualityLevel)
    {
        ItemUniqueName = itemUniqueName;
        QualityLevel = qualityLevel;
    }

    public string ItemUniqueName { get; }

    public int QualityLevel { get; }
}