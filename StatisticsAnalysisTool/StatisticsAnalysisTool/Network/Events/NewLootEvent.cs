using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewLootEvent : BaseEvent
    {
        public NewLootEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                Debug.Print($"{parameters[0]} {parameters[1]} {parameters[2]} {parameters[3]} {parameters[4]} {parameters[5]} {parameters[6]}");

                var containerId = parameters[0].ToString();
                // 2 = mob id?
                var bodyName = (string) parameters[3];

                var container = new Container
                {
                    Id = containerId, Owner = bodyName,
                    Type = BodyName.StartsWith("@MOB") ? ContainerType.Monster : ContainerType.Player
                };

                Debug.Print($"NewLoot - Id: {container.Id}, Type: {container.Type}");
            }
            catch(Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }

        public string ContainerId { get; }
        public string BodyName { get; }
    }
}