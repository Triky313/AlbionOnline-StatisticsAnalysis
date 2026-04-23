using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.StorageHistory;

internal static class VaultInfoParser
{
    private const int GuidByteLength = 16;

    public static List<Guid> GetGuids(object parameter)
    {
        if (parameter is byte[] vaultGuidBytes)
        {
            return GetGuidsFromBytes(vaultGuidBytes);
        }

        if (parameter is object[] vaultGuidArray)
        {
            return GetGuidsFromObjects(vaultGuidArray);
        }

        return [];
    }

    public static List<string> GetStrings(object parameter)
    {
        if (parameter is string[] stringArray)
        {
            return [.. stringArray];
        }

        if (parameter is object[] objectArray)
        {
            var values = new List<string>();

            foreach (var item in objectArray)
            {
                values.Add(item?.ToString() ?? string.Empty);
            }

            return values;
        }

        if (parameter is string singleString)
        {
            return [singleString];
        }

        return [];
    }

    public static List<int> GetInts(object parameter)
    {
        if (parameter is int[] intArray)
        {
            return [.. intArray];
        }

        if (parameter is object[] objectArray)
        {
            var values = new List<int>();

            foreach (var item in objectArray)
            {
                values.Add(item.ObjectToInt());
            }

            return values;
        }

        return [];
    }

    private static List<Guid> GetGuidsFromBytes(byte[] vaultGuidBytes)
    {
        var vaultGuids = new List<Guid>();
        var vaultCount = vaultGuidBytes.Length / GuidByteLength;

        for (var i = 0; i < vaultCount; i++)
        {
            var guidBytes = new byte[GuidByteLength];
            Array.Copy(vaultGuidBytes, i * GuidByteLength, guidBytes, 0, GuidByteLength);
            vaultGuids.Add(new Guid(guidBytes));
        }

        return vaultGuids;
    }

    private static List<Guid> GetGuidsFromObjects(object[] vaultGuidArray)
    {
        var vaultGuids = new List<Guid>();

        foreach (var vaultGuid in vaultGuidArray)
        {
            var guid = vaultGuid.ObjectToGuid();
            if (guid != null)
            {
                vaultGuids.Add(guid.Value);
            }
        }

        return vaultGuids;
    }
}
