using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class OtherGrabbedLootEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public Loot Loot;

        private readonly string _lootedBody;
        private readonly string _looterName;
        private readonly bool _isSilver;
        private readonly int _itemId;
        private readonly int _quantity;

        public OtherGrabbedLootEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(1))
                {
                    _lootedBody = MobController.IsMob(parameters[1].ToString()) ? LanguageController.Translation("MOB") : parameters[1].ToString();
                }

                if (parameters.ContainsKey(2))
                {
                    _looterName = parameters[2].ToString();
                }

                if (parameters.ContainsKey(3))
                {
                    _isSilver = parameters[3].ObjectToBool();
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
                    IsSilver = _isSilver,
                    Quantity = _quantity
                };
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}