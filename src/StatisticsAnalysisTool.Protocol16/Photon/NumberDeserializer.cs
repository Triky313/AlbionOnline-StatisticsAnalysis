namespace StatisticsAnalysisTool.Protocol16.Photon;

public class NumberDeserializer
{
    public static bool Deserialize(out int value, byte[] source, ref int offset)
    {
        value = 0;

        if (offset + 4 > source.Length)
        {
            return false;
        }

        var span = new Span<byte>(source, offset, 4);
        value = (span[0] << 24) | (span[1] << 16) | (span[2] << 8) | span[3];
        offset += 4;

        return true;
    }

    public static bool Deserialize(out short value, byte[] source, ref int offset)
    {
        value = 0;

        if (offset + 2 > source.Length)
        {
            return false;
        }

        var span = new Span<byte>(source, offset, 2);
        value = (short) ((span[0] << 8) | span[1]);
        offset += 2;

        return true;
    }
}