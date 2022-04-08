using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.ApiModel;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class LocalUserData
    {
        public long? UserObjectId { get; set; }
        public Guid? Guid { get; set; }
        public Guid? InteractGuid { get; set; }
        public string Username { get; set; }
        public FixPoint? LearningPoints { get; set; }
        public double? Reputation { get; set; }
        public FixPoint? ReSpecPoints { get; set; }
        public FixPoint? Silver { get; set; }
        public FixPoint? Gold { get; set; }
        public string GuildName { get; set; }
        public string MainMapIndex { get; set; }
        public int? PlayTimeInSeconds { get; set; }
        public string AllianceName { get; set; }
        public string WebApiUserId { get; set; }
        public List<GameInfoPlayerKillsDeaths> Kills { get; private set; }
        public List<GameInfoPlayerKillsDeaths> Deaths { get; private set; }
        public int KillsToday => Kills?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date) ?? 0;
        public int DeathsToday => Deaths?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date) ?? 0;
        public int KillsWeek => Kills?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7)) ?? 0;
        public int DeathsWeek => Deaths?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7)) ?? 0;
        public DateTime? LastUpdate;
        public double AverageItemPowerWhenKilling => Kills.Select(x => x?.Killer?.AverageItemPower).Sum() / Kills.Count ?? 0;
        public double AverageItemPowerOfTheKilledEnemies => Kills.Select(x => x?.Victim?.AverageItemPower).Sum() / Kills.Count ?? 0;
        public double AverageItemPowerWhenDying => Deaths.Select(x => x?.Victim?.AverageItemPower).Sum() / Deaths.Count ?? 0;

        public async Task SetValuesAsync(LocalUserData localUserData)
        {
            await GetApiData(Username, localUserData.Username);

            UserObjectId = localUserData.UserObjectId;
            Guid = localUserData.Guid;
            InteractGuid = localUserData.InteractGuid;
            Username = localUserData.Username;
            LearningPoints = localUserData.LearningPoints;
            Reputation = localUserData.Reputation;
            ReSpecPoints = localUserData.ReSpecPoints;
            Silver = localUserData.Silver;
            Gold = localUserData.Gold;
            GuildName = localUserData.GuildName;
            MainMapIndex = localUserData.MainMapIndex;
            PlayTimeInSeconds = localUserData.PlayTimeInSeconds;
            AllianceName = localUserData.AllianceName;
        }

        private async Task GetApiData(string currentUsername, string newUsername)
        {
            if (currentUsername != newUsername || LastUpdate < DateTime.UtcNow.AddMinutes(-5))
            {
                var info = await ApiController.GetGameInfoSearchFromJsonAsync(newUsername);
                WebApiUserId = info.SearchPlayer.FirstOrDefault(x => x.Name == newUsername)?.Id;

                Kills = await ApiController.GetGameInfoPlayerKillsDeathsFromJsonAsync(WebApiUserId, GameInfoPlayersType.Kills);
                Deaths = await ApiController.GetGameInfoPlayerKillsDeathsFromJsonAsync(WebApiUserId, GameInfoPlayersType.Deaths);

                LastUpdate = DateTime.UtcNow;
            }
        }
    } 
}