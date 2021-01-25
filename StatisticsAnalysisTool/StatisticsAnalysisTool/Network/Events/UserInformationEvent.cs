using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UserInformationEvent : BaseOperation
    {
        public UserInformationEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                //Debug.Print($"-----------------------------------------");
                //Debug.Print($"Response");

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
                    UniqueMapName = WorldController.GetUniqueNameNameOrDefault(MapIndex);
                    MapType = WorldController.GetMapType(MapIndex);
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
                    Silver = parameters.ContainsKey(28) ? long.Parse(parameters[28].ToString().Remove(parameters[28].ToString().Length - 4)) : 0;
                }

                if (parameters.ContainsKey(29))
                {
                    Gold = parameters.ContainsKey(29) ? long.Parse(parameters[29].ToString().Remove(parameters[29].ToString().Length - 4)) : 0;
                }

                if (parameters.ContainsKey(32))
                {
                    LearningPoints = parameters.ContainsKey(32) ? int.Parse(parameters[32].ToString().Remove(parameters[32].ToString().Length - 4)) : 0;
                }

                if (parameters.ContainsKey(36))
                {
                    Reputation = parameters.ContainsKey(36) ? int.Parse(parameters[36].ToString().Remove(parameters[36].ToString().Length - 4)) : 0;
                }

                if (parameters.ContainsKey(38))
                {
                    var array = (long[])parameters[38];
                    if (array != null && array.Length > 1)
                    {
                        RespecPoints = array[1];
                    }
                }

                if (parameters.ContainsKey(51))
                {
                    GuildName = string.IsNullOrEmpty(parameters[51].ToString()) ? string.Empty : parameters[51].ToString();
                }

                if (parameters.ContainsKey(69))
                {
                    AllianceName = string.IsNullOrEmpty(parameters[69].ToString()) ? string.Empty : parameters[69].ToString();
                }

                // Test
                if (parameters.ContainsKey(71))
                {
                    Test = string.IsNullOrEmpty(parameters[71].ToString()) ? string.Empty : parameters[71].ToString();
                }

                if (parameters.ContainsKey(92))
                {
                    CurrentDailyBonusPoints = parameters.ContainsKey(92) ? long.Parse(parameters[92].ToString().Remove(parameters[92].ToString().Length - 4)) : 0;
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public string Username { get; }
        public string MapIndex { get; }
        public string UniqueMapName { get; }
        public MapType MapType { get; }
        public long CurrentFocusPoints { get; }
        public long MaxCurrentFocusPoints { get; }
        public int LearningPoints { get; }
        public int Reputation { get; }
        public long RespecPoints { get; }
        public long Silver { get; }
        public long Gold { get; }
        public string GuildName { get; }
        public string AllianceName { get; }
        public string Test { get; }
        public long CurrentDailyBonusPoints { get; }
    }
}