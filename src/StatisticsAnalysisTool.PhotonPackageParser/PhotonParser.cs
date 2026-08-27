using StatisticsAnalysisTool.Abstractions;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Protocol18;
using StatisticsAnalysisTool.Protocol18.Photon;
using System.Buffers;

namespace StatisticsAnalysisTool.PhotonPackageParser;

public abstract class PhotonParser : IPhotonReceiver
{
    private const int CommandHeaderLength = 12;
    private const int PhotonHeaderLength = 12;
    private const int MaxFragmentedPayloadLength = 16 * 1024 * 1024;
    private const int MaxFragmentCount = 16 * 1024;
    private const int MaxPendingFragmentAssemblies = 256;
    private const int MaxPendingFragmentBytes = 64 * 1024 * 1024;
    private const int FragmentAssemblyTimeoutMilliseconds = 15_000;
    private const int FragmentCleanupIntervalMilliseconds = 1_000;

    private readonly Lock _fragmentLock = new();
    private readonly Dictionary<PhotonFragmentKey, PhotonFragmentAssembly> _pendingFragments = new();
    private int _pendingFragmentBytes;
    private long _nextFragmentCleanupTimestamp;

    public void ReceivePacket(byte[] payload)
    {
        ReceivePacket(payload.AsSpan());
    }

    public void ReceivePacket(ReadOnlySpan<byte> payload)
    {
        CleanupExpiredFragmentAssemblies();

        int packetOffset = 0;
        while (packetOffset < payload.Length)
        {
            ReadOnlySpan<byte> remainingPayload = payload[packetOffset..];
            if (!PhotonPacketFramer.TryReadPacketLength(remainingPayload, out int packetLength))
            {
                ReportMalformedPhotonPayload(packetOffset, remainingPayload.Length);
                return;
            }

            ReceiveSinglePacket(remainingPayload[..packetLength]);
            packetOffset += packetLength;
        }
    }

    public void ReceivePacket(ReadOnlySequence<byte> payload)
    {
        if (payload.Length == 0)
        {
            return;
        }

        if (payload.IsSingleSegment)
        {
            ReceivePacket(payload.FirstSpan);
            return;
        }

        var len = checked((int) payload.Length);
        var tmp = new byte[len];
        payload.CopyTo(tmp);
        ReceivePacket(tmp);
    }

    protected abstract void OnRequest(byte operationCode, Dictionary<byte, object> parameters);

    protected abstract void OnResponse(byte operationCode, short returnCode, string debugMessage, Dictionary<byte, object> parameters);

    protected abstract void OnEvent(byte code, Dictionary<byte, object> parameters);

    private void ReceiveSinglePacket(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < PhotonHeaderLength)
        {
            return;
        }

        int offset = 0;

        if (!NumberDeserializer.Deserialize(out short peerId, payload, ref offset))
        {
            return;
        }

        if (!ReadByte(out byte flags, payload, ref offset))
        {
            return;
        }

        if (!ReadByte(out byte commandCount, payload, ref offset))
        {
            return;
        }

        if (!NumberDeserializer.Deserialize(out int _, payload, ref offset))
        {
            return;
        }

        if (!NumberDeserializer.Deserialize(out int challenge, payload, ref offset))
        {
            return;
        }

        bool isEncrypted = flags == 1;
        bool isCrcEnabled = flags == 0xCC;

        if (isEncrypted)
        {
            // Encrypted packages are not supported
            return;
        }

        if (isCrcEnabled)
        {
            int crcOffset = offset;
            if (!NumberDeserializer.Deserialize(out int crc, payload, ref offset))
            {
                return;
            }

            if (unchecked((uint) crc) != CrcCalculator.Calculate(payload, crcOffset, sizeof(int)))
            {
                // Invalid crc
                return;
            }
        }

        for (int commandIndex = 0; commandIndex < commandCount; commandIndex++)
        {
            if (!HandleCommand(payload, ref offset, peerId, challenge))
            {
                return;
            }
        }
    }

    private bool HandleCommand(ReadOnlySpan<byte> source, ref int offset, short peerId, int challenge)
    {
        int commandStart = offset;
        if (commandStart < 0 || commandStart > source.Length - CommandHeaderLength)
        {
            return false;
        }

        if (!ReadByte(out byte commandType, source, ref offset))
        {
            return false;
        }

        if (!ReadByte(out byte channelId, source, ref offset))
        {
            return false;
        }

        if (!ReadByte(out byte _, source, ref offset))
        {
            return false;
        }

        offset++;
        if (!NumberDeserializer.Deserialize(out int commandLength, source, ref offset))
        {
            return false;
        }

        if (!NumberDeserializer.Deserialize(out int _, source, ref offset))
        {
            return false;
        }

        if (commandLength < CommandHeaderLength || commandLength > source.Length - commandStart)
        {
            return false;
        }

        int commandEnd = commandStart + commandLength;
        int commandPayloadLength = commandLength - CommandHeaderLength;

        switch ((CommandType) commandType)
        {
            case CommandType.Disconnect:
                break;
            case CommandType.SendUnreliable:
                if (commandPayloadLength < sizeof(int))
                {
                    return false;
                }

                offset += sizeof(int);
                commandPayloadLength -= sizeof(int);
                HandleSendReliable(source, ref offset, commandPayloadLength);
                break;
            case CommandType.SendReliable:
                HandleSendReliable(source, ref offset, commandPayloadLength);
                break;
            case CommandType.SendFragment:
                HandleSendFragment(source, ref offset, commandPayloadLength, peerId, challenge, channelId);
                break;
        }

        offset = commandEnd;
        return true;
    }

    private void HandleSendReliable(ReadOnlySpan<byte> source, ref int offset, int commandLength)
    {
        if (commandLength < 2 || offset < 0 || offset > source.Length - commandLength)
        {
            return;
        }

        offset++;
        if (!ReadByte(out byte messageType, source, ref offset))
        {
            return;
        }

        int operationLength = commandLength - 2;
        ReadOnlySpan<byte> operationPayload = source.Slice(offset, operationLength);
        offset += operationLength;

        switch ((MessageType) messageType)
        {
            case MessageType.OperationRequest:
                {
                    OperationRequest requestData = Protocol18Deserializer.DeserializeOperationRequest(operationPayload);
                    DebugConsole.LogOperationRequest(requestData.OperationCode, requestData.Parameters);
                    OnRequest(requestData.OperationCode, requestData.Parameters);
                    break;
                }
            case MessageType.OperationResponse:
                {
                    OperationResponse responseData = Protocol18Deserializer.DeserializeOperationResponse(operationPayload);
                    DebugConsole.LogOperationResponse(responseData.OperationCode, responseData.ReturnCode, responseData.DebugMessage, responseData.Parameters);
                    OnResponse(responseData.OperationCode, responseData.ReturnCode, responseData.DebugMessage, responseData.Parameters);
                    break;
                }
            case MessageType.Event:
                {
                    EventData eventData = Protocol18Deserializer.DeserializeEventData(operationPayload);
                    DebugConsole.LogEvent(eventData.Code, eventData.Parameters);
                    OnEvent(eventData.Code, eventData.Parameters);
                    break;
                }
        }
    }

    private void HandleSendFragment(ReadOnlySpan<byte> source, ref int offset, int commandLength, short peerId, int challenge, byte channelId)
    {
        const int fragmentHeaderLength = 5 * sizeof(int);
        if (commandLength <= fragmentHeaderLength || offset < 0 || offset > source.Length - commandLength)
        {
            return;
        }

        if (!NumberDeserializer.Deserialize(out int startSequenceNumber, source, ref offset)
            || !NumberDeserializer.Deserialize(out int fragmentCount, source, ref offset)
            || !NumberDeserializer.Deserialize(out int fragmentNumber, source, ref offset)
            || !NumberDeserializer.Deserialize(out int totalLength, source, ref offset)
            || !NumberDeserializer.Deserialize(out int fragmentOffset, source, ref offset))
        {
            return;
        }

        int fragmentLength = commandLength - fragmentHeaderLength;
        if (totalLength <= 0
            || totalLength > MaxFragmentedPayloadLength
            || fragmentCount <= 0
            || fragmentCount > MaxFragmentCount
            || fragmentNumber < 0
            || fragmentNumber >= fragmentCount
            || fragmentOffset < 0
            || fragmentOffset > totalLength
            || fragmentLength <= 0
            || fragmentLength > totalLength - fragmentOffset
            || fragmentLength > source.Length - offset)
        {
            ReportInvalidFragment(startSequenceNumber, fragmentNumber, fragmentCount);
            return;
        }

        var key = new PhotonFragmentKey(peerId, challenge, channelId, startSequenceNumber);
        ReadOnlySpan<byte> fragmentPayload = source.Slice(offset, fragmentLength);
        offset += fragmentLength;
        HandleFragment(key, totalLength, fragmentCount, fragmentNumber, fragmentOffset, fragmentPayload);
    }

    private void HandleFragment(
        PhotonFragmentKey key,
        int totalLength,
        int fragmentCount,
        int fragmentNumber,
        int fragmentOffset,
        ReadOnlySpan<byte> fragmentPayload)
    {
        byte[]? finishedPayload = null;
        int discardedAssemblies = 0;
        int missingFragments = 0;
        long timestamp = Environment.TickCount64;

        lock (_fragmentLock)
        {
            if (_pendingFragments.TryGetValue(key, out PhotonFragmentAssembly? assembly)
                && !assembly.IsCompatible(totalLength, fragmentCount))
            {
                RemoveFragmentAssembly(key, assembly);
                discardedAssemblies++;
                missingFragments += assembly.MissingFragmentCount;
                assembly = null;
            }

            if (assembly is null)
            {
                TrimFragmentAssemblies(totalLength, ref discardedAssemblies, ref missingFragments);
                assembly = new PhotonFragmentAssembly(totalLength, fragmentCount, timestamp);
                _pendingFragments.Add(key, assembly);
                _pendingFragmentBytes += totalLength;
            }

            if (!assembly.TryAddFragment(fragmentNumber, fragmentOffset, fragmentPayload, timestamp))
            {
                RemoveFragmentAssembly(key, assembly);
                discardedAssemblies++;
                missingFragments += assembly.MissingFragmentCount;
            }
            else if (assembly.IsComplete)
            {
                RemoveFragmentAssembly(key, assembly);
                finishedPayload = assembly.TotalPayload;
            }
        }

        if (discardedAssemblies > 0)
        {
            ReportFragmentLoss(discardedAssemblies, missingFragments, "invalid or resource-limited");
        }

        if (finishedPayload is not null)
        {
            HandleFinishedFragmentedPackage(finishedPayload);
        }
    }

    private void HandleFinishedFragmentedPackage(byte[] totalPayload)
    {
        int offset = 0;
        HandleSendReliable(totalPayload, ref offset, totalPayload.Length);
    }

    private void CleanupExpiredFragmentAssemblies()
    {
        long timestamp = Environment.TickCount64;
        if (timestamp < Volatile.Read(ref _nextFragmentCleanupTimestamp))
        {
            return;
        }

        int expiredAssemblies = 0;
        int missingFragments = 0;

        lock (_fragmentLock)
        {
            if (timestamp < _nextFragmentCleanupTimestamp)
            {
                return;
            }

            _nextFragmentCleanupTimestamp = timestamp + FragmentCleanupIntervalMilliseconds;
            if (_pendingFragments.Count == 0)
            {
                return;
            }

            List<PhotonFragmentKey>? expiredKeys = null;
            foreach (var pair in _pendingFragments)
            {
                if (timestamp - pair.Value.LastSeenTimestamp < FragmentAssemblyTimeoutMilliseconds)
                {
                    continue;
                }

                expiredKeys ??= [];
                expiredKeys.Add(pair.Key);
                expiredAssemblies++;
                missingFragments += pair.Value.MissingFragmentCount;
            }

            if (expiredKeys is not null)
            {
                foreach (PhotonFragmentKey key in expiredKeys)
                {
                    if (_pendingFragments.Remove(key, out PhotonFragmentAssembly? assembly))
                    {
                        _pendingFragmentBytes -= assembly.TotalLength;
                    }
                }
            }
        }

        if (expiredAssemblies > 0)
        {
            ReportFragmentLoss(expiredAssemblies, missingFragments, "timed out");
        }
    }

    private void TrimFragmentAssemblies(int requiredBytes, ref int discardedAssemblies, ref int missingFragments)
    {
        while (_pendingFragments.Count >= MaxPendingFragmentAssemblies
               || _pendingFragmentBytes > MaxPendingFragmentBytes - requiredBytes)
        {
            bool foundAssembly = false;
            PhotonFragmentKey oldestKey = default;
            PhotonFragmentAssembly? oldestAssembly = null;

            foreach (var pair in _pendingFragments)
            {
                if (oldestAssembly is not null && pair.Value.LastSeenTimestamp >= oldestAssembly.LastSeenTimestamp)
                {
                    continue;
                }

                foundAssembly = true;
                oldestKey = pair.Key;
                oldestAssembly = pair.Value;
            }

            if (!foundAssembly || oldestAssembly is null)
            {
                return;
            }

            RemoveFragmentAssembly(oldestKey, oldestAssembly);
            discardedAssemblies++;
            missingFragments += oldestAssembly.MissingFragmentCount;
        }
    }

    private void RemoveFragmentAssembly(PhotonFragmentKey key, PhotonFragmentAssembly assembly)
    {
        if (_pendingFragments.Remove(key))
        {
            _pendingFragmentBytes -= assembly.TotalLength;
        }
    }

    private static void ReportMalformedPhotonPayload(int packetOffset, int remainingLength)
    {
        if (!DebugConsole.IsAttached)
        {
            return;
        }

        DebugConsole.WriteWarn(typeof(PhotonParser), $"Photon payload is incomplete or malformed. Offset={packetOffset}, RemainingBytes={remainingLength}");
    }

    private static void ReportInvalidFragment(int startSequenceNumber, int fragmentNumber, int fragmentCount)
    {
        if (!DebugConsole.IsAttached)
        {
            return;
        }

        DebugConsole.WriteWarn(typeof(PhotonParser), $"Photon fragment was rejected. StartSequence={startSequenceNumber}, Fragment={fragmentNumber}/{fragmentCount}");
    }

    private static void ReportFragmentLoss(int assemblyCount, int missingFragmentCount, string reason)
    {
        if (!DebugConsole.IsAttached)
        {
            return;
        }

        DebugConsole.WriteWarn(typeof(PhotonParser), $"Photon fragment assemblies were discarded. Reason={reason}, Assemblies={assemblyCount}, MissingFragments={missingFragmentCount}");
    }

    private static bool ReadByte(out byte value, ReadOnlySpan<byte> source, ref int offset)
    {
        value = 0;

        if (offset < 0 || offset >= source.Length)
        {
            return false;
        }

        value = source[offset++];
        return true;
    }
}