namespace Protocol16.Photon
{
    public class NumberDeserializer
    {
        public static void Deserialize(out int value, byte[] source, ref int offset)
        {
            int value1 = source[offset] << 24;
            offset++;
            int value2 = value1 | source[offset] << 16;
            offset++;
            int value3 = value2 | source[offset] << 8;
            offset++;
            value = (value3 | source[offset]);
            offset++;
        }

        public static void Deserialize(out short value, byte[] source, ref int offset)
        {
            short value1 = (short)(source[offset] << 8);
            offset++;
            value = (short)(value1 | (short)source[offset]);
            offset++;
        }
    }
}
