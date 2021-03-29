using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewLootChestEvent : BaseEvent
    {
        public NewLootChestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0) && int.TryParse(parameters[0].ToString(), out var id))
                {
                    Id = id;
                }

                if (parameters.ContainsKey(3))
                {
                    UniqueName = string.IsNullOrEmpty(parameters[3].ToString()) ? string.Empty : parameters[3].ToString();
                }

                //if (parameters.ContainsKey(4))
                //{
                //    UniqueNameArea? = string.IsNullOrEmpty(parameters[4].ToString()) ? string.Empty : parameters[4].ToString();
                //}
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public int Id { get; set; }

        public string UniqueName { get; set; }
    }
}