using System.Collections.Generic;

namespace StatisticsAnalysisTool.Streaming;

/// <summary>JSON returned by /api/stats. Only values the tool already shows.</summary>
public sealed class StreamingSnapshot
{
    public StreamingConfig Config { get; set; } = new();
    public Dictionary<string, string> Labels { get; set; } = new();
    public StreamingSession Session { get; set; } = new();
    public List<StreamingDamageEntry> DamageMeter { get; set; } = [];
    public StreamingGathering Gathering { get; set; } = new();
}

public sealed class StreamingConfig
{
    public bool ShowSession { get; set; }
    public bool ShowDamageMeter { get; set; }
    public bool ShowGathering { get; set; }
    public int MaxPlayers { get; set; }
    public double FontSizePercentage { get; set; }
    public double BackgroundOpacityPercentage { get; set; }
}

public sealed class StreamingSession
{
    public double SessionSeconds { get; set; }
    public List<StreamingStat> Stats { get; set; } = [];
}

public sealed class StreamingStat
{
    public string Key { get; set; }
    public string Icon { get; set; }
    public string Label { get; set; }
    public double PerHour { get; set; }
    public double Total { get; set; }
    /// <summary>Formatted like the tool: short total ("2,05K") and full value per hour ("1,107,248/h").</summary>
    public string TotalText { get; set; }
    public string PerHourText { get; set; }
}

public sealed class StreamingDamageEntry
{
    public string Name { get; set; }
    public string WeaponUniqueName { get; set; }
    public long Damage { get; set; }
    public double Dps { get; set; }
    public string DamageText { get; set; }
    public string DpsText { get; set; }
    public double DamagePercentage { get; set; }
    public double DamageBarPercentage { get; set; }
    public long Heal { get; set; }
    public double Hps { get; set; }
}

public sealed class StreamingGathering
{
    public bool IsActive { get; set; }
    public double SilverPerHour { get; set; }
    public long TotalSilver { get; set; }
    public long TotalResources { get; set; }
    public StreamingStat Summary { get; set; }
    public List<StreamingStat> Resources { get; set; } = [];
}
