namespace StatisticsAnalysisTool.Protocol18;

internal static class Protocol18StreamExtensions
{
    public static void WriteTypeCodeIfTrue(this Protocol18Stream output, Protocol16Type type, bool writeTypeCode)
    {
        if (writeTypeCode)
        {
            output.WriteByte((byte) type);
        }
    }
}