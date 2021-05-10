using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewMobEvent : BaseEvent
    {
        public NewMobEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Console.WriteLine($@"[{DateTime.UtcNow}] {GetType().Name}: {JsonConvert.SerializeObject(parameters)}");

            Debug.Print("--- NewMob (Event) ---");
            //foreach (var parameter in parameters)
            //{
            //    Debug.Print($"{parameter}");
            //}

            try
            {
                if (parameters.ContainsKey(0)) ObjectId = parameters[0].ObjectToLong();

                if (parameters.ContainsKey(1)) Type = parameters[1].ObjectToLong() ?? 0;

                Debug.Print($"ObjectId: {ObjectId} | Type: {Type}");
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public long? ObjectId { get; }
        public long Type { get; }
    }
}