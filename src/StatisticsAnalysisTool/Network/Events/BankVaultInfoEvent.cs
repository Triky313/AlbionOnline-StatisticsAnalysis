using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class BankVaultInfoEvent
{
    private const int GuidByteLength = 16;

    public long? ObjectId;
    public string LocationGuidString;
    public List<Guid> VaultGuidList = [];
    public List<string> VaultNames = [];
    public List<string> IconTags = [];
    public List<int> VaultColors = [];

    public BankVaultInfoEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object locationGuid))
            {
                LocationGuidString = locationGuid.ToString();
            }

            if (parameters.TryGetValue(2, out object vaultGuids))
            {
                VaultGuidList = GetVaultGuids(vaultGuids);
            }

            if (parameters.TryGetValue(3, out object vaultNames))
            {
                VaultNames = GetStringValues(vaultNames);
            }

            if (parameters.TryGetValue(4, out object iconTags))
            {
                IconTags = GetStringValues(iconTags);
            }

            if (parameters.TryGetValue(5, out object vaultColors))
            {
                VaultColors = GetIntValues(vaultColors);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private static List<Guid> GetVaultGuids(object parameter)
    {
        if (parameter is byte[] vaultGuidBytes)
        {
            return GetVaultGuidsFromBytes(vaultGuidBytes);
        }

        if (parameter is object[] vaultGuidArray)
        {
            return GetVaultGuidsFromObjects(vaultGuidArray);
        }

        return new List<Guid>();
    }

    private static List<Guid> GetVaultGuidsFromBytes(byte[] vaultGuidBytes)
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

    private static List<Guid> GetVaultGuidsFromObjects(object[] vaultGuidArray)
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

    private static List<string> GetStringValues(object parameter)
    {
        if (parameter is string[] stringArray)
        {
            return new List<string>(stringArray);
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

        return new List<string>();
    }

    private static List<int> GetIntValues(object parameter)
    {
        if (parameter is int[] intArray)
        {
            return new List<int>(intArray);
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

        return new List<int>();
    }
}