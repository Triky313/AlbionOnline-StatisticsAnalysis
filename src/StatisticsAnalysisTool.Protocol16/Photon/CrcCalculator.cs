namespace Protocol16.Photon
{
    public static class CrcCalculator
    {
        public static uint Calculate(byte[] bytes, int length)
        {
            uint result = uint.MaxValue;
            uint key = 3988292384u;

            for (int i = 0; i < length; i++)
            {
                result ^= bytes[i];
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
}
