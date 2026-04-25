using System;

namespace StatisticsAnalysisTool.Network.PacketProviders;

public sealed class NetworkCaptureException : Exception
{
    public NetworkCaptureException(string message)
        : base(message)
    {
    }

    public NetworkCaptureException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
