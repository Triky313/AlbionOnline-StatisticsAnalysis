using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network;

internal static class NetworkParameterParser
{
    public static IReadOnlyList<long> GetLongValues(object value)
    {
        return value switch
        {
            long[] values => values,
            int[] values => Array.ConvertAll(values, x => (long) x),
            short[] values => Array.ConvertAll(values, x => (long) x),
            byte[] values => Array.ConvertAll(values, x => (long) x),
            long singleValue => [singleValue],
            int singleValue => [singleValue],
            short singleValue => [singleValue],
            byte singleValue => [singleValue],
            _ => []
        };
    }

    public static IReadOnlyList<int> GetIntValues(object value)
    {
        var values = GetLongValues(value);
        var result = new List<int>(values.Count);
        foreach (var currentValue in values)
        {
            if (currentValue is >= int.MinValue and <= int.MaxValue)
            {
                result.Add((int) currentValue);
            }
        }

        return result;
    }
}