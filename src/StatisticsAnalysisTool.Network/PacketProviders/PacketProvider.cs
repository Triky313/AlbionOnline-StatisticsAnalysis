#nullable enable

using System;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public abstract class PacketProvider
{
    private string _activeAdapterName = string.Empty;

    public event EventHandler<string?>? ActiveAdapterChanged;

    public string ActiveAdapterName => _activeAdapterName;

    public abstract bool IsRunning { get; }

    public abstract void Start();

    public abstract void Stop();

    protected void UpdateActiveAdapterName(string? activeAdapterName)
    {
        string normalizedName = activeAdapterName?.Trim() ?? string.Empty;
        if (string.Equals(_activeAdapterName, normalizedName, StringComparison.Ordinal))
        {
            return;
        }

        _activeAdapterName = normalizedName;
        ActiveAdapterChanged?.Invoke(this, string.IsNullOrWhiteSpace(_activeAdapterName) ? null : _activeAdapterName);
    }
}
