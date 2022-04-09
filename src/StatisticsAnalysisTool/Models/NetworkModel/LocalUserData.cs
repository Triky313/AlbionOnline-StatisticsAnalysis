using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
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
        public DateTime? LastUpdate;
        public List<GameInfoPlayerKillsDeaths> TopKillsMonthValues { get; private set; }
        public List<GameInfoPlayerKillsDeaths> SoloKillsMonthValues { get; private set; }
        public List<GameInfoPlayerKillsDeaths> DeathsValues { get; private set; }

        public int SoloKillsToday => SoloKillsMonthValues?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date) ?? 0;
        public int SoloKillsWeek => SoloKillsMonthValues?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7)) ?? 0;
        public int SoloKillsMonth => SoloKillsMonthValues?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-30)) ?? 0;
        public int KillsToday => TopKillsMonthValues?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date) ?? 0;
        public int KillsWeek => TopKillsMonthValues?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7)) ?? 0;
        public int KillsMonth => TopKillsMonthValues?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-30)) ?? 0;
        public int DeathsToday => DeathsValues?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date) ?? 0;
        public int DeathsWeek => DeathsValues?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7)) ?? 0;
        public int DeathsMonth => DeathsValues?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-30)) ?? 0;
        public double AverageItemPowerWhenKilling => TopKillsMonthValues.Select(x => x?.Killer?.AverageItemPower).Sum() / TopKillsMonthValues.Count ?? 0;
        public double AverageItemPowerOfTheKilledEnemies => TopKillsMonthValues.Select(x => x?.Victim?.AverageItemPower).Sum() / TopKillsMonthValues.Count ?? 0;
        public double AverageItemPowerWhenDying => DeathsValues.Select(x => x?.Victim?.AverageItemPower).Sum() / DeathsValues.Count ?? 0;

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
                
                DeathsValues = await ApiController.GetGameInfoPlayerKillsDeathsFromJsonAsync(WebApiUserId, GameInfoPlayersType.Deaths);
                TopKillsMonthValues = await ApiController.GetGameInfoPlayerTopKillsFromJsonAsync(WebApiUserId, UnitOfTime.Month);
                SoloKillsMonthValues = await ApiController.GetGameInfoPlayerSoloKillsFromJsonAsync(WebApiUserId, UnitOfTime.Month);

                LastUpdate = DateTime.UtcNow;
            }
        }
    } 
}