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

    private readonly Dictionary<int, SegmentedPackage> _pendingSegments = new();

    public void ReceivePacket(byte[] payload)
    {
        ReceivePacket(payload.AsSpan());
    }

    public void ReceivePacket(ReadOnlySpan<byte> payload)
    {
        if (payload.Length < PhotonHeaderLength)
        {
            return;
        }

        int offset = 0;

        if (!NumberDeserializer.Deserialize(out short _, payload, ref offset))
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

        if (!NumberDeserializer.Deserialize(out int _, payload, ref offset))
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
            int ignoredOffset = 0;
            if (!NumberDeserializer.Deserialize(out int crc, payload, ref ignoredOffset))
            {
                return;
            }

            int crcOffset = offset;
            if (crcOffset > payload.Length - sizeof(int))
            {
                return;
            }

            offset += sizeof(int);

            if (crc != CrcCalculator.Calculate(payload, crcOffset, sizeof(int)))
            {
                // Invalid crc
                return;
            }
        }

        for (int commandIdx = 0; commandIdx < commandCount; commandIdx++)
        {
            HandleCommand(payload, ref offset);
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

    private void HandleCommand(ReadOnlySpan<byte> source, ref int offset)
    {
        if (!ReadByte(out byte commandType, source, ref offset))
        {
            return;
        }
        if (!ReadByte(out byte _, source, ref offset))
        {
            return;
        }
        if (!ReadByte(out byte _, source, ref offset))
        {
            return;
        }
        // Skip 1 byte
        offset++;
        if (!NumberDeserializer.Deserialize(out int commandLength, source, ref offset))
        {
            return;
        }
        if (!NumberDeserializer.Deserialize(out int _, source, ref offset))
        {
            return;
        }
        commandLength -= CommandHeaderLength;

        switch ((CommandType) commandType)
        {
            case CommandType.Disconnect:
                {
                    return;
                }
            case CommandType.SendUnreliable:
                {
                    offset += 4;
                    commandLength -= 4;
                    goto case CommandType.SendReliable;
                }
            case CommandType.SendReliable:
                {
                    HandleSendReliable(source, ref offset, ref commandLength);
                    break;
                }
            case CommandType.SendFragment:
                {
                    HandleSendFragment(source, ref offset, ref commandLength);
                    break;
                }
            default:
                {
                    offset += commandLength;
                    break;
                }
        }
    }

    private void HandleSendReliable(ReadOnlySpan<byte> source, ref int offset, ref int commandLength)
    {
        // Skip 1 byte
        offset++;
        commandLength--;
        ReadByte(out byte messageType, source, ref offset);
        commandLength--;

        int operationLength = commandLength;
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

    private void HandleSendFragment(ReadOnlySpan<byte> source, ref int offset, ref int commandLength)
    {
        if (!NumberDeserializer.Deserialize(out int startSequenceNumber, source, ref offset))
        {
            return;
        }
        commandLength -= 4;
        if (!NumberDeserializer.Deserialize(out int _, source, ref offset))
        {
            return;
        }
        commandLength -= 4;
        if (!NumberDeserializer.Deserialize(out int _, source, ref offset))
        {
            return;
        }
        commandLength -= 4;
        if (!NumberDeserializer.Deserialize(out int totalLength, source, ref offset))
        {
            return;
        }
        commandLength -= 4;
        if (!NumberDeserializer.Deserialize(out int fragmentOffset, source, ref offset))
        {
            return;
        }
        commandLength -= 4;

        int fragmentLength = commandLength;
        if (totalLength <= 0 || fragmentLength <= 0)
        {
            return;
        }

        HandleSegmentedPayload(startSequenceNumber, totalLength, fragmentLength, fragmentOffset, source, ref offset);
    }

    private void HandleFinishedSegmentedPackage(byte[] totalPayload)
    {
        int offset = 0;
        int commandLength = totalPayload.Length;
        HandleSendReliable(totalPayload, ref offset, ref commandLength);
    }

    private void HandleSegmentedPayload(int startSequenceNumber, int totalLength, int fragmentLength, int fragmentOffset, ReadOnlySpan<byte> source, ref int offset)
    {
        SegmentedPackage segmentedPackage = GetSegmentedPackage(startSequenceNumber, totalLength);

        if (fragmentOffset < 0 || fragmentLength <= 0 || fragmentOffset > segmentedPackage.TotalLength)
        {
            _pendingSegments.Remove(startSequenceNumber);
            return;
        }

        if (fragmentLength > segmentedPackage.TotalLength - fragmentOffset)
        {
            _pendingSegments.Remove(startSequenceNumber);
            return;
        }

        if (offset < 0 || offset > source.Length || fragmentLength > source.Length - offset)
        {
            _pendingSegments.Remove(startSequenceNumber);
            return;
        }

        source.Slice(offset, fragmentLength)
            .CopyTo(segmentedPackage.TotalPayload.AsSpan(fragmentOffset, fragmentLength));
        offset += fragmentLength;

        int fragmentEnd = fragmentOffset + fragmentLength;
        for (int index = fragmentOffset; index < fragmentEnd; index++)
        {
            if (segmentedPackage.ReceivedBytes[index])
            {
                continue;
            }

            segmentedPackage.ReceivedBytes[index] = true;
            segmentedPackage.ReceivedBytesCount++;
        }

        if (segmentedPackage.ReceivedBytesCount >= segmentedPackage.TotalLength)
        {
            _pendingSegments.Remove(startSequenceNumber);
            HandleFinishedSegmentedPackage(segmentedPackage.TotalPayload);
        }
    }

    private SegmentedPackage GetSegmentedPackage(int startSequenceNumber, int totalLength)
    {
        if (_pendingSegments.TryGetValue(startSequenceNumber, out SegmentedPackage? segmentedPackage))
        {
            if (segmentedPackage != null && segmentedPackage.TotalLength != totalLength)
            {
                _pendingSegments.Remove(startSequenceNumber);
                segmentedPackage = new SegmentedPackage(totalLength);
                _pendingSegments.Add(startSequenceNumber, segmentedPackage);
            }

            if (segmentedPackage != null)
            {
                return segmentedPackage;
            }
        }

        segmentedPackage = new SegmentedPackage(totalLength);
        _pendingSegments.Add(startSequenceNumber, segmentedPackage);

        return segmentedPackage;
    }

    private bool ReadByte(out byte value, ReadOnlySpan<byte> source, ref int offset)
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
