namespace Protocol16.Photon
{
    public class NumberSerializer
    {
        public static void Serialize(int value, byte[] target, ref int offset)
        {
            target[offset] = (byte)(value >> 24);
            offset++;
            target[offset] = (byte)(value >> 16);
            offset++;
            target[offset] = (byte)(value >> 8);
            offset++;
            target[offset] = (byte)value;
            offset++;
        }
    }
}
