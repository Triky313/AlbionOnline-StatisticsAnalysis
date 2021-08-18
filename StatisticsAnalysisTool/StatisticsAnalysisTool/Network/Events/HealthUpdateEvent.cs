using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class HealthUpdateEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public long CauserId;
        public int CausingSpellType;
        public EffectOrigin EffectOrigin;
        public EffectType EffectType;
        public double HealthChange;
        public double NewHealthValue;

        public long ObjectId;
        public GameTimeStamp TimeStamp;

        public HealthUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0))
                {
                    ObjectId = parameters[0].ObjectToLong() ?? throw new ArgumentNullException();
                }

                if (parameters.ContainsKey(1))
                {
                    TimeStamp = new GameTimeStamp(parameters[1].ObjectToLong() ?? 0);
                }

                if (parameters.ContainsKey(2))
                {
                    HealthChange = parameters[2].ObjectToDouble();
                }

                if (parameters.ContainsKey(3))
                {
                    NewHealthValue = parameters[3].ObjectToDouble();
                }

                if (parameters.ContainsKey(4))
                {
                    EffectType = (EffectType) (parameters[4] as byte? ?? 0);
                }

                if (parameters.ContainsKey(5))
                {
                    EffectOrigin = (EffectOrigin) (parameters[5] as byte? ?? 0);
                }

                if (parameters.ContainsKey(6))
                {
                    CauserId = parameters[6].ObjectToLong() ?? throw new ArgumentNullException();
                }

                if (parameters.ContainsKey(7))
                {
                    CausingSpellType = parameters[7].ObjectToShort();
                }
            }
            catch (ArgumentNullException ex)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}