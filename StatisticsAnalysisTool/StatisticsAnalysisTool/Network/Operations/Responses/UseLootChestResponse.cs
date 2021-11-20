using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class UseLootChestResponse
    {
        public DiscoveredLoot Loot;

        //private readonly long? _objectId;
        //private readonly string _deadPlayer;
        //private readonly string _looter;
        //private readonly int _quantity;
        //private readonly int _itemId;

        public UseLootChestResponse(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                //if (parameters.ContainsKey(0))
                //{
                //    _objectId = parameters[0].ObjectToLong();
                //}

                //if (parameters.ContainsKey(1))
                //{
                //    _deadPlayer = parameters[1].ToString();
                //}

                //if (parameters.ContainsKey(2))
                //{
                //    _looter = parameters[2].ToString();
                //}

                //if (parameters.ContainsKey(4))
                //{
                //    _itemId = parameters[4].ObjectToInt();
                //}

                //if (parameters.ContainsKey(5))
                //{
                //    _quantity = parameters[5].ObjectToInt();
                //}

                //if (_objectId != null)
                //{
                //    Loot = new DiscoveredLoot()
                //    {
                //        ObjectId = (long)_objectId,
                //        ItemIndex = _itemId,
                //        LootedBody = _deadPlayer,
                //        LooterName = _looter,
                //        Quantity = _quantity
                //        //Item = ItemController.GetItemByIndex(_itemId),
                //        //IsTrash = ItemController.IsTrash(_itemId)
                //    };
                //}
                //else
                //{
                //    Loot = null;
                //}
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}