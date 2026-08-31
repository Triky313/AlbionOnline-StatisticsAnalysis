using FluentAssertions;
using NUnit.Framework;
using StatisticsAnalysisTool.PhotonPackageParser;
using StatisticsAnalysisTool.Protocol18.Photon;
using System.Buffers.Binary;

namespace StatisticsAnalysisTool.UnitTests.PhotonPackageParser;

[TestFixture]
public class PhotonParserTests
{
    private const short PeerId = unchecked((short) 0xF100);
    private const int Challenge = 123456;

    [Test]
    public void ReceivePacket_WithCoalescedPhotonPackets_ProcessesEveryPacket()
    {
        var parser = new TestPhotonParser();
        byte[] firstPacket = BuildOperationResponsePacket(174, false);
        byte[] secondPacket = BuildOperationResponsePacket(176, true);
        byte[] coalescedPayload = [.. firstPacket, .. secondPacket];

        parser.ReceivePacket(coalescedPayload);

        parser.ResponseOperationCodes.Should().Equal(174, 176);
    }

    [Test]
    public void ReceivePacket_WithLargeOutOfOrderFragments_ReassemblesResponseOnce()
    {
        var parser = new TestPhotonParser();
        byte[] expectedData = Enumerable.Range(0, 40_000).Select(index => (byte) (index % 251)).ToArray();
        byte[] reliablePayload = BuildReliableResponsePayload(174, expectedData);
        IReadOnlyList<byte[]> fragments = BuildFragmentPackets(reliablePayload, 900, 4000, 1);

        parser.ReceivePacket(fragments[^1]);
        parser.ReceivePacket(fragments[^1]);

        for (int index = fragments.Count - 2; index >= 0; index--)
        {
            parser.ReceivePacket(fragments[index]);
        }

        parser.ResponseOperationCodes.Should().ContainSingle().Which.Should().Be(174);
        parser.ResponseParameters.Should().ContainSingle();
        parser.ResponseParameters[0][1].Should().BeEquivalentTo(expectedData);
    }

    [Test]
    public void ReceivePacket_WithSameSequenceOnDifferentChannels_KeepsAssembliesSeparate()
    {
        var parser = new TestPhotonParser();
        byte[] firstPayload = BuildReliableResponsePayload(174, Enumerable.Repeat((byte) 1, 4_000).ToArray());
        byte[] secondPayload = BuildReliableResponsePayload(176, Enumerable.Repeat((byte) 2, 4_000).ToArray());
        IReadOnlyList<byte[]> firstFragments = BuildFragmentPackets(firstPayload, 700, 5000, 1);
        IReadOnlyList<byte[]> secondFragments = BuildFragmentPackets(secondPayload, 700, 5000, 2);

        for (int index = 0; index < firstFragments.Count; index++)
        {
            parser.ReceivePacket(firstFragments[index]);
            parser.ReceivePacket(secondFragments[index]);
        }

        parser.ResponseOperationCodes.Should().Equal(174, 176);
    }

    [Test]
    public void ReceivePacket_WithMissingFragment_DoesNotEmitPartialResponse()
    {
        var parser = new TestPhotonParser();
        byte[] reliablePayload = BuildReliableResponsePayload(174, new byte[5_000]);
        IReadOnlyList<byte[]> fragments = BuildFragmentPackets(reliablePayload, 700, 6000, 1);

        for (int index = 0; index < fragments.Count; index++)
        {
            if (index != 2)
            {
                parser.ReceivePacket(fragments[index]);
            }
        }

        parser.ResponseOperationCodes.Should().BeEmpty();
    }

    [Test]
    public void TryReadPacketLength_WithTruncatedCommand_ReturnsFalse()
    {
        byte[] packet = BuildOperationResponsePacket(174, false);

        bool result = PhotonPacketFramer.TryReadPacketLength(packet.AsSpan(0, packet.Length - 1), out int packetLength);

        result.Should().BeFalse();
        packetLength.Should().Be(0);
    }

    private static byte[] BuildOperationResponsePacket(byte operationCode, bool useCrc)
    {
        return BuildPhotonPacket(6, 1, BuildReliableResponsePayload(operationCode, null), 1, useCrc);
    }

    private static byte[] BuildReliableResponsePayload(byte operationCode, byte[]? data)
    {
        List<byte> payload =
        [
            0,
            3,
            operationCode,
            0,
            0,
            8
        ];

        if (data is null)
        {
            payload.Add(0);
            return payload.ToArray();
        }

        payload.Add(1);
        payload.Add(1);
        payload.Add(67);
        WriteCompressedUInt32(payload, checked((uint) data.Length));
        payload.AddRange(data);
        return payload.ToArray();
    }

    private static IReadOnlyList<byte[]> BuildFragmentPackets(
        byte[] totalPayload,
        int fragmentSize,
        int startSequenceNumber,
        byte channelId)
    {
        int fragmentCount = (totalPayload.Length + fragmentSize - 1) / fragmentSize;
        var result = new List<byte[]>(fragmentCount);

        for (int fragmentNumber = 0; fragmentNumber < fragmentCount; fragmentNumber++)
        {
            int fragmentOffset = fragmentNumber * fragmentSize;
            int currentLength = Math.Min(fragmentSize, totalPayload.Length - fragmentOffset);
            var commandPayload = new byte[5 * sizeof(int) + currentLength];

            BinaryPrimitives.WriteInt32BigEndian(commandPayload.AsSpan(0, sizeof(int)), startSequenceNumber);
            BinaryPrimitives.WriteInt32BigEndian(commandPayload.AsSpan(4, sizeof(int)), fragmentCount);
            BinaryPrimitives.WriteInt32BigEndian(commandPayload.AsSpan(8, sizeof(int)), fragmentNumber);
            BinaryPrimitives.WriteInt32BigEndian(commandPayload.AsSpan(12, sizeof(int)), totalPayload.Length);
            BinaryPrimitives.WriteInt32BigEndian(commandPayload.AsSpan(16, sizeof(int)), fragmentOffset);
            totalPayload.AsSpan(fragmentOffset, currentLength).CopyTo(commandPayload.AsSpan(20));

            result.Add(BuildPhotonPacket(8, channelId, commandPayload, fragmentNumber + 1, false));
        }

        return result;
    }

    private static byte[] BuildPhotonPacket(
        byte commandType,
        byte channelId,
        byte[] commandPayload,
        int sequenceNumber,
        bool useCrc)
    {
        int photonHeaderLength = useCrc ? 16 : 12;
        int commandLength = 12 + commandPayload.Length;
        var packet = new byte[photonHeaderLength + commandLength];

        BinaryPrimitives.WriteInt16BigEndian(packet.AsSpan(0, sizeof(short)), PeerId);
        packet[2] = useCrc ? (byte) 0xCC : (byte) 0;
        packet[3] = 1;
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(4, sizeof(int)), 1000);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(8, sizeof(int)), Challenge);

        int commandOffset = photonHeaderLength;
        packet[commandOffset] = commandType;
        packet[commandOffset + 1] = channelId;
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(commandOffset + 4, sizeof(int)), commandLength);
        BinaryPrimitives.WriteInt32BigEndian(packet.AsSpan(commandOffset + 8, sizeof(int)), sequenceNumber);
        commandPayload.CopyTo(packet.AsSpan(commandOffset + 12));

        if (useCrc)
        {
            uint crc = CrcCalculator.Calculate(packet, 12, sizeof(int));
            BinaryPrimitives.WriteUInt32BigEndian(packet.AsSpan(12, sizeof(int)), crc);
        }

        return packet;
    }

    private static void WriteCompressedUInt32(List<byte> target, uint value)
    {
        do
        {
            byte current = (byte) (value & 0x7F);
            value >>= 7;
            if (value != 0)
            {
                current |= 0x80;
            }

            target.Add(current);
        }
        while (value != 0);
    }

    private sealed class TestPhotonParser : PhotonParser
    {
        public List<byte> ResponseOperationCodes { get; } = [];

        public List<Dictionary<byte, object>> ResponseParameters { get; } = [];

        protected override void OnRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
        }

        protected override void OnResponse(
            byte operationCode,
            short returnCode,
            string debugMessage,
            Dictionary<byte, object> parameters)
        {
            ResponseOperationCodes.Add(operationCode);
            ResponseParameters.Add(parameters);
        }

        protected override void OnEvent(byte code, Dictionary<byte, object> parameters)
        {
        }
    }
}
