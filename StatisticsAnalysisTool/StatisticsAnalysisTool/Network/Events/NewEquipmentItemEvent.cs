using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewEquipmentItemEvent : BaseEvent
    {
        public NewEquipmentItemEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(1))
                {
                    ItemIndex = parameters[1].ObjectToInt();
                }

                if (parameters.ContainsKey(8))
                {
                    var valueType = parameters[8].GetType();
                    if (valueType.IsArray && typeof(int[]).Name == valueType.Name)
                    {
                        var spells = ((int[])parameters[2]).ToDictionary();
                        SpellDictionary.Add(0, spells[0].ObjectToInt());
                        SpellDictionary.Add(1, spells[1].ObjectToInt());
                        SpellDictionary.Add(2, spells[2].ObjectToInt());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public int ItemIndex { get; }
        public Dictionary<int, int> SpellDictionary { get; } = new Dictionary<int, int>();
    }
}