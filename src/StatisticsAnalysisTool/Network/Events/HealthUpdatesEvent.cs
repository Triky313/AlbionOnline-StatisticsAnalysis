using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.Converters;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class HealthUpdatesEvent
{
    public long AffectedObjectId;
    public List<HealthUpdate> HealthUpdates { get; } = new();

    public HealthUpdatesEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            Dictionary<int, long> causerIds = new();
            Dictionary<int, double> healthChanges = new();
            Dictionary<int, double> newHealthValues = new();
            Dictionary<int, byte> effectTypes = new();
            Dictionary<int, byte> effectOrigins = new();
            Dictionary<int, short> causingSpellIndices = new();

            if (parameters.TryGetValue(0, out object affectedObjectId))
            {
                AffectedObjectId = affectedObjectId.ObjectToLong() ?? throw new ArgumentNullException();
            }

            if (parameters.TryGetValue(2, out object healthChangesParameters))
            {
                healthChanges = Converter.GetValue<double>(healthChangesParameters);
            }

            if (parameters.TryGetValue(3, out object newHealthValuesParameters))
            {
                newHealthValues = Converter.GetValue<double>(newHealthValuesParameters);
            }

            if (parameters.TryGetValue(4, out object effectTypesParameters))
            {
                effectTypes = Converter.GetValue<byte>(effectTypesParameters);
            }

            if (parameters.TryGetValue(5, out object effectOriginsParameters))
            {
                effectOrigins = Converter.GetValue<byte>(effectOriginsParameters);
            }

            if (parameters.TryGetValue(6, out object causerIdParameters))
            {
                causerIds = Converter.GetValue<long>(causerIdParameters);
            }

            if (parameters.TryGetValue(7, out object causingSpellIndicesParameters))
            {
                causingSpellIndices = Converter.GetValue<short>(causingSpellIndicesParameters);
            }

            int maxCount = new[]
            {
                healthChanges?.Count ?? 0, newHealthValues?.Count ?? 0, effectTypes?.Count ?? 0, effectOrigins?.Count ?? 0, causerIds?.Count ?? 0, causingSpellIndices?.Count ?? 0
            }.Max();

            for (int i = 0; i < maxCount; i++)
            {
                HealthUpdate healthUpdate = new HealthUpdate
                {
                    AffectedObjectId = AffectedObjectId,
                    HealthChange = i < healthChanges.Count ? healthChanges[i] : 0,
                    NewHealthValue = i < newHealthValues.Count ? newHealthValues[i] : 0,
                    EffectType = i < effectTypes.Count ? (EffectType) effectTypes[i] : EffectType.None,
                    EffectOrigin = i < effectOrigins.Count ? (EffectOrigin) effectOrigins[i] : EffectOrigin.Unknown,
                    CauserId = i < causerIds.Count ? causerIds[i] : 0,
                    CausingSpellIndex = i < causingSpellIndices.Count ? causingSpellIndices[i] : 0
                };

                HealthUpdates.Add(healthUpdate);
            }
        }
        catch (ArgumentNullException ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}