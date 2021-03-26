using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class JoinResponse : BaseOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public long? UserObjectId;

        public JoinResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                Debug.Print("---------- UserInformation (Response) ----------");
                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}

                UserObjectId = null;
                if (parameters.ContainsKey(0))
                {
                    UserObjectId = parameters[0].ObjectToLong();
                    Debug.Print($"Local user ObjectId: {UserObjectId}");
                }

                if (parameters.ContainsKey(1))
                {
                    Guid = parameters[1].ObjectToGuid();
                    Debug.Print($"Local user Guid: {Guid}");
                }

                if (parameters.ContainsKey(2)) Username = parameters[2].ToString();

                if (parameters.ContainsKey(8))
                {
                    MapIndex = parameters[8].ToString();
                    MapType = WorldData.GetMapType(MapIndex);
                    DungeonGuid = WorldData.GetDungeonGuid(MapIndex);
                }

                if (parameters.ContainsKey(23)) CurrentFocusPoints = parameters[23].ObjectToDouble();

                if (parameters.ContainsKey(24)) MaxCurrentFocusPoints = parameters[24].ObjectToDouble();

                if (parameters.ContainsKey(28)) Silver = FixPoint.FromInternalValue(parameters[28].ObjectToLong() ?? 0);

                if (parameters.ContainsKey(29)) Gold = FixPoint.FromInternalValue(parameters[29].ObjectToLong() ?? 0);

                if (parameters.ContainsKey(32)) LearningPoints = FixPoint.FromInternalValue(parameters[32].ObjectToLong() ?? 0);

                if (parameters.ContainsKey(36)) Reputation = parameters[36].ObjectToDouble();

                if (parameters.ContainsKey(38) && parameters[38] != null && parameters[38] is long[] reSpecArray && reSpecArray.Length > 1)
                    ReSpecPoints = FixPoint.FromInternalValue(reSpecArray[1]);

                if (parameters.ContainsKey(51))
                    GuildName = string.IsNullOrEmpty(parameters[51].ToString()) ? string.Empty : parameters[51].ToString();

                if (parameters.ContainsKey(58))
                    MainMapIndex = string.IsNullOrEmpty(parameters[58].ToString()) ? string.Empty : parameters[58].ToString();

                if (parameters.ContainsKey(61)) PlayTimeInSeconds = parameters[61].ObjectToInt();

                if (parameters.ContainsKey(69))
                    AllianceName = string.IsNullOrEmpty(parameters[69].ToString()) ? string.Empty : parameters[69].ToString();

                if (parameters.ContainsKey(92)) CurrentDailyBonusPoints = parameters[92].ObjectToLong();
            }
            catch (Exception e)
            {
                Log.Debug(nameof(JoinResponse), e);
            }
        }

        public Guid? Guid { get; }
        public string Username { get; }
        public string MapIndex { get; }
        public Guid? DungeonGuid { get; }
        public MapType MapType { get; }
        public double CurrentFocusPoints { get; }
        public double MaxCurrentFocusPoints { get; }
        public FixPoint LearningPoints { get; }
        public double Reputation { get; }
        public FixPoint ReSpecPoints { get; }
        public FixPoint Silver { get; }
        public FixPoint Gold { get; }
        public string GuildName { get; }
        public string MainMapIndex { get; set; }
        public int PlayTimeInSeconds { get; set; }
        public string AllianceName { get; }
        public long? CurrentDailyBonusPoints { get; }
    }
}