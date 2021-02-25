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
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public HealthUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                //Debug.Print($"----- HealthUpdate (Event) -----");
                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}

                if (parameters.ContainsKey(0))
                {
                    ObjectId = parameters[0].ObjectToLong();
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
                    CauserId = parameters[6].ObjectToLong();
                }

                if (parameters.ContainsKey(7))
                {
                    CausingSpellType = parameters[7].ObjectToShort();
                }

                //Debug.Print($"---------------------------");
                //Debug.Print($"ObjectId: {ObjectId}");
                //Debug.Print($"TimeStamp: {TimeStamp}");
                //Debug.Print($"HealthChange: {HealthChange}");
                //Debug.Print($"NewHealthValue: {NewHealthValue}");
                //Debug.Print($"EffectType: {EffectType}");
                //Debug.Print($"EffectOrigin: {EffectOrigin}");
                //Debug.Print($"CauserId: {CauserId}");
                //Debug.Print($"CausingSpellType: {CausingSpellType}");
            }
            catch (Exception e)
            {
                Log.Error(nameof(UpdateMoneyEvent), e);
            }
        }

        public long? ObjectId;
        public GameTimeStamp TimeStamp;
        public double HealthChange;
        public double NewHealthValue;
        public EffectType EffectType;
        public EffectOrigin EffectOrigin;
        public long? CauserId;
        public int CausingSpellType;
    }
}