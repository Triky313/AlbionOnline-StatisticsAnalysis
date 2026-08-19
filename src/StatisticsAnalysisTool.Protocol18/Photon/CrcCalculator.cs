namespace StatisticsAnalysisTool.Protocol18.Photon;

public static class CrcCalculator
{
    public static uint Calculate(byte[] bytes, int length)
    {
        return Calculate(bytes.AsSpan(0, length));
    }

    public static uint Calculate(ReadOnlySpan<byte> bytes)
    {
        return Calculate(bytes, bytes.Length, 0);
    }

    public static uint Calculate(ReadOnlySpan<byte> bytes, int zeroedOffset, int zeroedLength)
    {
        if (zeroedOffset < 0 || zeroedLength < 0 || zeroedOffset > bytes.Length - zeroedLength)
        {
            throw new ArgumentOutOfRangeException(nameof(zeroedOffset));
        }

        int zeroedEnd = zeroedOffset + zeroedLength;
        uint result = uint.MaxValue;
        const uint key = 3988292384u;

        for (int i = 0; i < bytes.Length; i++)
        {
            byte value = i >= zeroedOffset && i < zeroedEnd ? (byte) 0 : bytes[i];
            result ^= value;

            for (int j = 0; j < 8; j++)
            {
                if ((result & 1u) > 0u)
                {
                    result = result >> 1 ^ key;
                }
                else
                {
                    result >>= 1;
                }
            }
        }

        return result;
    }
}
