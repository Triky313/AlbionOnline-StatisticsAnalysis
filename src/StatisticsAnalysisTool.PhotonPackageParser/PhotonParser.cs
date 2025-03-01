using StatisticsAnalysisTool.Protocol16;
using StatisticsAnalysisTool.Protocol16.Photon;

namespace StatisticsAnalysisTool.PhotonPackageParser;

public abstract class PhotonParser
{
    private const int CommandHeaderLength = 12;
    private const int PhotonHeaderLength = 12;

    private readonly Dictionary<int, SegmentedPackage?> _pendingSegments = new();

    public void ReceivePacket(byte[] payload)
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
        if (!NumberDeserializer.Deserialize(out int timestamp, payload, ref offset))
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
            int ignoredOffset = 0;
            if (!NumberDeserializer.Deserialize(out int crc, payload, ref ignoredOffset))
            {
                return;
            }
            NumberSerializer.Serialize(0, payload, ref offset);

            if (crc != CrcCalculator.Calculate(payload, payload.Length))
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

    protected abstract void OnRequest(byte operationCode, Dictionary<byte, object> parameters);

    protected abstract void OnResponse(byte operationCode, short returnCode, string debugMessage, Dictionary<byte, object> parameters);

    protected abstract void OnEvent(byte code, Dictionary<byte, object> parameters);

    private void HandleCommand(byte[] source, ref int offset)
    {
        if (!ReadByte(out byte commandType, source, ref offset))
        {
            return;
        }
        if (!ReadByte(out byte channelId, source, ref offset))
        {
            return;
        }
        if (!ReadByte(out byte commandFlags, source, ref offset))
        {
            return;
        }
        // Skip 1 byte
        offset++;
        if (!NumberDeserializer.Deserialize(out int commandLength, source, ref offset))
        {
            return;
        }
        if (!NumberDeserializer.Deserialize(out int sequenceNumber, source, ref offset))
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

    private void HandleSendReliable(byte[] source, ref int offset, ref int commandLength)
    {
        // Skip 1 byte
        offset++;
        commandLength--;
        ReadByte(out byte messageType, source, ref offset);
        commandLength--;

        int operationLength = commandLength;
        var payload = new Protocol16Stream(operationLength);
        payload.Write(source, offset, operationLength);
        payload.Seek(0L, SeekOrigin.Begin);

        offset += operationLength;
        switch ((MessageType) messageType)
        {
            case MessageType.OperationRequest:
                {
                    OperationRequest requestData = Protocol16Deserializer.DeserializeOperationRequest(payload);
                    OnRequest(requestData.OperationCode, requestData.Parameters);
                    break;
                }
            case MessageType.OperationResponse:
                {
                    OperationResponse responseData = Protocol16Deserializer.DeserializeOperationResponse(payload);
                    OnResponse(responseData.OperationCode, responseData.ReturnCode, responseData.DebugMessage, responseData.Parameters);
                    break;
                }
            case MessageType.Event:
                {
                    EventData eventData = Protocol16Deserializer.DeserializeEventData(payload);
                    OnEvent(eventData.Code, eventData.Parameters);
                    break;
                }
        }
    }

    private void HandleSendFragment(byte[] source, ref int offset, ref int commandLength)
    {
        NumberDeserializer.Deserialize(out int startSequenceNumber, source, ref offset);
        commandLength -= 4;
        NumberDeserializer.Deserialize(out int fragmentCount, source, ref offset);
        commandLength -= 4;
        NumberDeserializer.Deserialize(out int fragmentNumber, source, ref offset);
        commandLength -= 4;
        NumberDeserializer.Deserialize(out int totalLength, source, ref offset);
        commandLength -= 4;
        NumberDeserializer.Deserialize(out int fragmentOffset, source, ref offset);
        commandLength -= 4;

        int fragmentLength = commandLength;
        HandleSegmentedPayload(startSequenceNumber, totalLength, fragmentLength, fragmentOffset, source, ref offset);
    }

    private void HandleFinishedSegmentedPackage(byte[] totalPayload)
    {
        int offset = 0;
        int commandLength = totalPayload.Length;
        HandleSendReliable(totalPayload, ref offset, ref commandLength);
    }

    private void HandleSegmentedPayload(int startSequenceNumber, int totalLength, int fragmentLength, int fragmentOffset, byte[] source, ref int offset)
    {
        SegmentedPackage? segmentedPackage = GetSegmentedPackage(startSequenceNumber, totalLength);

        Buffer.BlockCopy(source, offset, segmentedPackage.TotalPayload, fragmentOffset, fragmentLength);
        offset += fragmentLength;
        segmentedPackage.BytesWritten += fragmentLength;

        if (segmentedPackage.BytesWritten >= segmentedPackage.TotalLength)
        {
            _pendingSegments.Remove(startSequenceNumber);
            HandleFinishedSegmentedPackage(segmentedPackage.TotalPayload);
        }
    }

    private SegmentedPackage? GetSegmentedPackage(int startSequenceNumber, int totalLength)
    {
        if (_pendingSegments.TryGetValue(startSequenceNumber, out SegmentedPackage? segmentedPackage))
        {
            return segmentedPackage;
        }

        segmentedPackage = new SegmentedPackage
        {
            TotalLength = totalLength,
            TotalPayload = new byte[totalLength],
        };
        _pendingSegments.Add(startSequenceNumber, segmentedPackage);

        return segmentedPackage;
    }

    private bool ReadByte(out byte value, byte[] source, ref int offset)
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