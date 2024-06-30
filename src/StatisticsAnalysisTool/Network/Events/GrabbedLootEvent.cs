using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class GrabbedLootEvent
{
    public readonly Loot Loot;

    private readonly string _lootedFromName;
    private readonly string _looterByName;
    private readonly bool _isSilver;
    private readonly int _itemIndex;
    private readonly int _quantity;

    public GrabbedLootEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.ContainsKey(1))
            {
                _lootedFromName = MobController.IsMob(parameters[1].ToString()) ? LocalizationController.Translation("MOB") : parameters[1].ToString();
            }

            if (parameters.ContainsKey(2))
            {
                _looterByName = parameters[2].ToString();
            }

            if (parameters.ContainsKey(3))
            {
                _isSilver = parameters[3].ObjectToBool();
            }

            if (parameters.ContainsKey(4))
            {
                _itemIndex = parameters[4].ObjectToInt();
            }

            if (parameters.ContainsKey(5))
            {
                _quantity = parameters[5].ObjectToInt();
            }

            Loot = new Loot()
            {
                LootedFromName = _lootedFromName,
                IsTrash = ItemController.IsTrash(_itemIndex) && !_isSilver,
                ItemIndex = _itemIndex,
                LootedByName = _looterByName,
                IsSilver = _isSilver,
                Quantity = _quantity
            };
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}