using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class GuildVaultInfoEvent
{
    private const int GuidByteLength = 16;

    public long? ObjectId;
    public string LocationGuidString;
    public List<Guid> VaultGuidList = [];
    public List<string> VaultNames = [];
    public List<string> IconTags = [];

    public GuildVaultInfoEvent(Dictionary<byte, object> parameters)
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

        if (parameter is object[] vaultGuidArray)
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

        return [];
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

        if (parameter is string singleString)
        {
            return [singleString];
        }

        return [];
    }
}