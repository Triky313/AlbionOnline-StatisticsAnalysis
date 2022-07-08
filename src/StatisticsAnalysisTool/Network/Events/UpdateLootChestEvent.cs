using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class UpdateLootChestEvent
    {
        public int ObjectId { get; set; }
        public Guid PlayerGuid { get; set; }
        public Guid PlayerGuid2 { get; set; }

        public UpdateLootChestEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0) && int.TryParse(parameters[0].ToString(), out var objectId))
                {
                    ObjectId = objectId;
                }

                if (parameters.ContainsKey(3))
                {
                    PlayerGuid = ((object[])parameters[3]).FirstOrDefault().ObjectToGuid() ?? Guid.Empty;
                }

                if (parameters.ContainsKey(4))
                {
                    PlayerGuid2 = PlayerGuid = ((object[])parameters[4]).FirstOrDefault().ObjectToGuid() ?? Guid.Empty;
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}