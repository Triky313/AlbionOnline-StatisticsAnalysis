using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewEquipmentItemEvent
    {
        public DiscoveredItem Item;

        private readonly long? _objectId;
        private readonly int _itemId;
        private readonly int _quantity;
        private Dictionary<int, int> _spellDictionary { get; } = new ();

        public NewEquipmentItemEvent(Dictionary<byte, object> parameters)
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

                if (parameters.ContainsKey(8))
                {
                    var valueType = parameters[8].GetType();
                    if (valueType.IsArray && typeof(byte[]).Name == valueType.Name)
                    {
                        var spells = ((byte[]) parameters[8]).ToDictionary();
                        foreach (var spell in spells)
                        {
                            _spellDictionary.Add(spell.Key, spell.Value.ObjectToInt());
                        }
                    }
                    else if (valueType.IsArray && typeof(short[]).Name == valueType.Name)
                    {
                        var spells = ((short[]) parameters[8]).ToDictionary();
                        foreach (var spell in spells)
                        {
                            _spellDictionary.Add(spell.Key, spell.Value.ObjectToInt());
                        }
                    }
                    else if (valueType.IsArray && typeof(int[]).Name == valueType.Name)
                    {
                        var spells = ((int[]) parameters[8]).ToDictionary();
                        foreach (var spell in spells)
                        {
                            _spellDictionary.Add(spell.Key, spell.Value.ObjectToInt());
                        }
                    }
                }

                if (_objectId != null)
                {
                    Item = new DiscoveredItem()
                    {
                        ObjectId = (long)_objectId,
                        ItemId = _itemId,
                        Quantity = _quantity,
                        SpellDictionary = _spellDictionary
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