using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class OtherGrabbedLootEvent : BaseEvent
    {
        public Loot Loot;

        private readonly string _lootedBody;
        private readonly string _looterName;
        private readonly int _itemId;
        private readonly int _quantity;

        public OtherGrabbedLootEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(1))
                {
                    _lootedBody = parameters[1].ToString();
                }

                if (parameters.ContainsKey(2))
                {
                    _looterName = parameters[2].ToString();
                }

                if (parameters.ContainsKey(4))
                {
                    _itemId = parameters[4].ObjectToInt();
                }

                if (parameters.ContainsKey(5))
                {
                    _quantity = parameters[5].ObjectToInt();
                }

                Loot = new Loot()
                {
                    LootedBody = _lootedBody,
                    IsTrash = ItemController.IsTrash(_itemId),
                    Item = ItemController.GetItemByIndex(_itemId),
                    ItemId = _itemId,
                    LooterName = _looterName,
                    Quantity = _quantity
                };
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }
    }
}