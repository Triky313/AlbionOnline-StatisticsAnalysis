namespace StatisticsAnalysisTool.Protocol16;

internal static class Protocol16StreamExtensions
{
    public static void WriteTypeCodeIfTrue(this Protocol16Stream output, Protocol16Type type, bool writeTypeCode)
    {
        if (writeTypeCode)
        {
            output.WriteByte((byte) type);
        }
    }
}
