using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class UserInformationResponse : BaseOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public UserInformationResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                //Debug.Print($"---------- Response ----------");

                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}

                if (parameters.ContainsKey(2))
                {
                    Username = parameters[2].ToString();
                }

                if (parameters.ContainsKey(8))
                {
                    MapIndex = parameters[8].ToString();
                    UniqueMapName = WorldController.GetUniqueNameOrDefault(MapIndex);
                    MapType = WorldController.GetMapType(MapIndex);
                    DungeonGuid = WorldController.GetDungeonGuid(MapIndex);
                }

                if (parameters.ContainsKey(23) && long.TryParse(parameters[23].ToString(), out long currentFocusPoints))
                {
                    CurrentFocusPoints = currentFocusPoints;
                }

                if (parameters.ContainsKey(24) && long.TryParse(parameters[24].ToString(), out long maxCurrentFocusPoints))
                {
                    MaxCurrentFocusPoints = maxCurrentFocusPoints;
                }

                if (parameters.ContainsKey(28))
                {
                    Silver = (long)parameters[28] / 10000d;
                }

                if (parameters.ContainsKey(29))
                {
                    Gold = (int)parameters[29] / 10000d;
                }

                if (parameters.ContainsKey(32))
                {
                    LearningPoints = (int)parameters[32] / 10000d;
                }

                if (parameters.ContainsKey(36) && Converter.ParseToDouble(parameters[36].ToString(), out double newReputation))
                {
                    Reputation = newReputation;
                }

                if (parameters.ContainsKey(38) && parameters[38] != null)
                {
                    var array = (long[])parameters[38];
                    if (array != null && array.Length > 1)
                    {
                        ReSpecPoints = array[1] / 10000d;
                    }
                }

                if (parameters.ContainsKey(51))
                {
                    GuildName = string.IsNullOrEmpty(parameters[51].ToString()) ? string.Empty : parameters[51].ToString();
                }

                if (parameters.ContainsKey(58))
                {
                    MainMapIndex = string.IsNullOrEmpty(parameters[58].ToString()) ? string.Empty : parameters[58].ToString();
                }

                if (parameters.ContainsKey(61))
                {
                    PlayTimeInSeconds = (int)parameters[61] / 10000d;
                }

                if (parameters.ContainsKey(69))
                {
                    AllianceName = string.IsNullOrEmpty(parameters[69].ToString()) ? string.Empty : parameters[69].ToString();
                }
                
                if (parameters.ContainsKey(92))
                {
                    CurrentDailyBonusPoints = parameters.ContainsKey(92) ? long.Parse(parameters[92].ToString().Remove(parameters[92].ToString().Length - 4)) : 0;
                }
            }
            catch (Exception e)
            {
                Log.Debug(nameof(UserInformationResponse), e);
            }
        }

        public string Username { get; }
        public string MapIndex { get; }
        public string UniqueMapName { get; }
        public Guid? DungeonGuid { get; }
        public MapType MapType { get; }
        public long CurrentFocusPoints { get; }
        public long MaxCurrentFocusPoints { get; }
        public double LearningPoints { get; }
        public double Reputation { get; }
        public double ReSpecPoints { get; }
        public double Silver { get; }
        public double Gold { get; }
        public string GuildName { get; }
        public string MainMapIndex { get; set; }
        public double PlayTimeInSeconds { get; set; }
        public string AllianceName { get; }
        public long CurrentDailyBonusPoints { get; }
    }
}