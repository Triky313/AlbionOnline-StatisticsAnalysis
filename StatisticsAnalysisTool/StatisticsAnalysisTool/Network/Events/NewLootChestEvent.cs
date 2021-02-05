using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewLootChestEvent : BaseEvent
    {
        public NewLootChestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print($"----- NewLootChestEvent -----");

            try
            {
                foreach (var parameter in parameters)
                {
                    Debug.Print($"{parameter}");
                }

                if (parameters.ContainsKey(3))
                {
                    Died = string.IsNullOrEmpty(parameters[3].ToString()) ? string.Empty : parameters[3].ToString();
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        private DungeonMode GetDungeonMode(string value)
        {
            var valuesArray = value.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

            if (valuesArray.Contains("SOLO"))
            {
                return DungeonMode.Solo;
            } 
            
            if(valuesArray.Contains("STANDARD"))
            {
                return DungeonMode.Standard;
            }
            
            if(valuesArray.Contains("AVALON"))
            {
                return DungeonMode.Avalon;
            }

            return DungeonMode.Unknown;
        }

        private DungeonMode GetChestRarity(string value)
        {
            var valuesArray = value.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);

            if (valuesArray.Contains("SOLO"))
            {
                return DungeonMode.Solo;
            } 
            
            if(valuesArray.Contains("BOOKCHEST_STANDARD"))
            {
                return DungeonMode.Standard;
            }
            
            if(valuesArray.Contains("AVALON"))
            {
                return DungeonMode.Avalon;
            }

            return DungeonMode.Unknown;
        }

        public string Died { get; set; }
    }
}