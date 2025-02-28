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

        int value1 = source[offset] << 24;
        offset++;
        int value2 = value1 | source[offset] << 16;
        offset++;
        int value3 = value2 | source[offset] << 8;
        offset++;
        value = (value3 | source[offset]);
        offset++;

        return true;
    }

    public static bool Deserialize(out short value, byte[] source, ref int offset)
    {
        value = 0;
        if (offset + 2 > source.Length)
        {
            return false;
        }

        short value1 = (short) (source[offset] << 8);
        offset++;
        value = (short) (value1 | (short) source[offset]);
        offset++;

        return true;
    }
}