using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewSimpleItemEvent : BaseEvent
    {
        public DiscoveredLoot Loot;

        private readonly long? _objectId;
        private readonly int _itemId;
        private readonly int _quantity;

        public NewSimpleItemEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0))
                {
                    _objectId = parameters[0].ObjectToLong();
                }

                if (parameters.ContainsKey(1))
                {
                    _itemId = parameters[1].ObjectToInt();
                }

                if (parameters.ContainsKey(2))
                {
                    _quantity = parameters[2].ObjectToInt();
                }

                if (_objectId != null)
                {
                    Loot = new DiscoveredLoot()
                    {
                        ObjectId = (long)_objectId,
                        ItemId = _itemId,
                        Quantity = _quantity
                    };
                }
                else
                {
                    Loot = null;
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }
    }
}