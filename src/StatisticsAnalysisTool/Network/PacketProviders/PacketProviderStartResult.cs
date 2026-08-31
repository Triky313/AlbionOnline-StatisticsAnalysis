using System;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public readonly record struct PacketProviderStartResult
{
    private PacketProviderStartResult(int activeCaptureSourceCount)
    {
        ActiveCaptureSourceCount = activeCaptureSourceCount;
    }

    public bool IsSuccessful => ActiveCaptureSourceCount > 0;

    public int ActiveCaptureSourceCount { get; }

    public static PacketProviderStartResult Failed => default;

    public static PacketProviderStartResult Success(int activeCaptureSourceCount)
    {
        if (activeCaptureSourceCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activeCaptureSourceCount));
        }

        return new PacketProviderStartResult(activeCaptureSourceCount);
    }
}