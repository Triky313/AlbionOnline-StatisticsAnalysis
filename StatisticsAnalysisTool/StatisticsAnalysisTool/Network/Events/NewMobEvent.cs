using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewMobEvent
    {
        public NewMobEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
            
            try
            {
                if (parameters.ContainsKey(0)) 
                    ObjectId = parameters[0].ObjectToLong();

                if (parameters.ContainsKey(1)) 
                    Type = parameters[1].ObjectToLong() ?? 0;

                if (parameters.ContainsKey(11))
                    MoveSpeed = parameters[11].ObjectToDouble();

                if (parameters.ContainsKey(13))
                    HitPoints = parameters[13].ObjectToInt();

                if (parameters.ContainsKey(14))
                    HitPointsMax = parameters[14].ObjectToInt();

                if (parameters.ContainsKey(17))
                    Energy = parameters[17].ObjectToInt();

                if (parameters.ContainsKey(18))
                    EnergyMax = parameters[18].ObjectToInt();

                if (parameters.ContainsKey(19))
                    EnergyRegeneration = parameters[19].ObjectToInt();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        public long? ObjectId { get; }
        public long Type { get; }
        public double MoveSpeed { get; }
        public int HitPoints { get; }
        public int HitPointsMax { get; }
        public int Energy { get; }
        public int EnergyMax { get; }
        public int EnergyRegeneration { get; }
    }
}