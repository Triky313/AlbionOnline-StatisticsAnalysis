using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Threading;

namespace StatisticsAnalysisTool.Network.PacketProviders;

internal sealed class NpcapCaptureDiagnostics
{
    private const int ReportIntervalMilliseconds = 5_000;

    private long _nextReportTimestamp;
    private long _truncatedFrameCount;
    private long _missingCapturedByteCount;
    private long _malformedPacketCount;
    private long _expiredIPv4AssemblyCount;
    private long _evictedIPv4AssemblyCount;

    public void RecordTruncatedFrame(long capturedLength, long declaredLength)
    {
        if (!DebugConsole.IsAttached)
        {
            return;
        }

        Interlocked.Increment(ref _truncatedFrameCount);
        Interlocked.Add(ref _missingCapturedByteCount, Math.Max(0, declaredLength - capturedLength));
        FlushIfDue();
    }

    public void RecordMalformedPacket()
    {
        if (!DebugConsole.IsAttached)
        {
            return;
        }

        Interlocked.Increment(ref _malformedPacketCount);
        FlushIfDue();
    }

    public void RecordExpiredIPv4Assemblies(int count)
    {
        if (!DebugConsole.IsAttached || count <= 0)
        {
            return;
        }

        Interlocked.Add(ref _expiredIPv4AssemblyCount, count);
        FlushIfDue();
    }

    public void RecordEvictedIPv4Assemblies(int count)
    {
        if (!DebugConsole.IsAttached || count <= 0)
        {
            return;
        }

        Interlocked.Add(ref _evictedIPv4AssemblyCount, count);
        FlushIfDue();
    }

    public void FlushIfDue()
    {
        if (!DebugConsole.IsAttached)
        {
            return;
        }

        long timestamp = Environment.TickCount64;
        long nextReportTimestamp = Volatile.Read(ref _nextReportTimestamp);
        if (timestamp < nextReportTimestamp)
        {
            return;
        }

        if (Interlocked.CompareExchange(ref _nextReportTimestamp, timestamp + ReportIntervalMilliseconds, nextReportTimestamp) != nextReportTimestamp)
        {
            return;
        }

        long truncatedFrames = Interlocked.Exchange(ref _truncatedFrameCount, 0);
        long missingCapturedBytes = Interlocked.Exchange(ref _missingCapturedByteCount, 0);
        long malformedPackets = Interlocked.Exchange(ref _malformedPacketCount, 0);
        long expiredIPv4Assemblies = Interlocked.Exchange(ref _expiredIPv4AssemblyCount, 0);
        long evictedIPv4Assemblies = Interlocked.Exchange(ref _evictedIPv4AssemblyCount, 0);

        if (truncatedFrames == 0
            && malformedPackets == 0
            && expiredIPv4Assemblies == 0
            && evictedIPv4Assemblies == 0)
        {
            return;
        }

        DebugConsole.WriteWarn(
            typeof(LibpcapPacketProvider),
            $"Npcap capture anomalies detected. TruncatedFrames={truncatedFrames}, MissingCapturedBytes={missingCapturedBytes}, MalformedPackets={malformedPackets}, ExpiredIPv4Assemblies={expiredIPv4Assemblies}, EvictedIPv4Assemblies={evictedIPv4Assemblies}");
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _nextReportTimestamp, 0);
        Interlocked.Exchange(ref _truncatedFrameCount, 0);
        Interlocked.Exchange(ref _missingCapturedByteCount, 0);
        Interlocked.Exchange(ref _malformedPacketCount, 0);
        Interlocked.Exchange(ref _expiredIPv4AssemblyCount, 0);
        Interlocked.Exchange(ref _evictedIPv4AssemblyCount, 0);
    }
}