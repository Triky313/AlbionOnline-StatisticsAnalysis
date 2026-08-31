using System.Buffers.Binary;

namespace StatisticsAnalysisTool.PhotonPackageParser;

internal static class PhotonPacketFramer
{
    private const int PhotonHeaderLength = 12;
    private const int CrcLength = 4;
    private const int CommandHeaderLength = 12;

    public static bool TryReadPacketLength(ReadOnlySpan<byte> payload, out int packetLength)
    {
        packetLength = 0;

        if (payload.Length < PhotonHeaderLength)
        {
            return false;
        }

        byte flags = payload[2];
        if (flags == 1)
        {
            packetLength = payload.Length;
            return true;
        }

        int offset = PhotonHeaderLength;
        if (flags == 0xCC)
        {
            offset += CrcLength;
        }

        if (offset > payload.Length)
        {
            return false;
        }

        byte commandCount = payload[3];
        for (int commandIndex = 0; commandIndex < commandCount; commandIndex++)
        {
            if (offset > payload.Length - CommandHeaderLength)
            {
                return false;
            }

            int commandLength = BinaryPrimitives.ReadInt32BigEndian(payload.Slice(offset + 4, sizeof(int)));
            if (commandLength < CommandHeaderLength || commandLength > payload.Length - offset)
            {
                return false;
            }

            offset += commandLength;
        }

        packetLength = offset;
        return true;
    }
}