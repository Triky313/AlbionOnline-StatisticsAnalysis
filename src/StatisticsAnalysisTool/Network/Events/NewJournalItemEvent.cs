using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewJournalItemEvent
    {
        public DiscoveredItem Item;

        private readonly long? _objectId;
        private readonly int _itemId;
        private readonly int _quantity;
        private readonly long _estimatedMarketValue;

        public NewJournalItemEvent(Dictionary<byte, object> parameters)
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

                if (parameters.ContainsKey(4))
                {
                    _estimatedMarketValue = parameters[4].ObjectToLong() ?? 0;
                }

                if (_objectId != null)
                {
                    Item = new DiscoveredItem()
                    {
                        ObjectId = (long)_objectId,
                        ItemIndex = _itemId,
                        Quantity = _quantity,
                        EstimatedMarketValueInternal = _estimatedMarketValue
                    };
                }
                else
                {
                    Item = null;
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}