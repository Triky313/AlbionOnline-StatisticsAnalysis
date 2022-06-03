using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.ApiModel;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class LocalUserData
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

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
        public ObservableCollection<GameInfoPlayerKillsDeathsWithType> PlayerKillsDeaths { get; private set; }

        public int SoloKillsToday => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date && x.ObjectType == GameInfoPlayerKillsDeathsType.SoloKill) ?? 0;
        public int SoloKillsWeek => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7) && x.ObjectType == GameInfoPlayerKillsDeathsType.SoloKill) ?? 0;
        public int SoloKillsMonth => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-30) && x.ObjectType == GameInfoPlayerKillsDeathsType.SoloKill) ?? 0;
        public int KillsToday => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date && x.ObjectType == GameInfoPlayerKillsDeathsType.Kill) ?? 0;
        public int KillsWeek => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7) && x.ObjectType == GameInfoPlayerKillsDeathsType.Kill) ?? 0;
        public int KillsMonth => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-30) && x.ObjectType == GameInfoPlayerKillsDeathsType.Kill) ?? 0;
        public int DeathsToday => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date == DateTime.UtcNow.Date && x.ObjectType == GameInfoPlayerKillsDeathsType.Death) ?? 0;
        public int DeathsWeek => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-7) && x.ObjectType == GameInfoPlayerKillsDeathsType.Death) ?? 0;
        public int DeathsMonth => PlayerKillsDeaths?.ToArray().Count(x => x.TimeStamp.Date > DateTime.UtcNow.Date.AddDays(-30) && x.ObjectType == GameInfoPlayerKillsDeathsType.Death) ?? 0;
        public double AverageItemPowerWhenKilling =>
            PlayerKillsDeaths?.Where(x => x.ObjectType != GameInfoPlayerKillsDeathsType.Death).Select(x => x.Killer?.AverageItemPower).Sum() / PlayerKillsDeaths?.Count(x => x.ObjectType != GameInfoPlayerKillsDeathsType.Death) ?? 0;
        public double AverageItemPowerOfTheKilledEnemies =>
            PlayerKillsDeaths?.Where(x => x.ObjectType != GameInfoPlayerKillsDeathsType.Death).Select(x => x.Victim?.AverageItemPower).Sum() / PlayerKillsDeaths?.Count(x => x.ObjectType != GameInfoPlayerKillsDeathsType.Death) ?? 0;
        public double AverageItemPowerWhenDying =>
            PlayerKillsDeaths?.Where(x => x.ObjectType == GameInfoPlayerKillsDeathsType.Death).Select(x => x.Victim?.AverageItemPower).Sum() / PlayerKillsDeaths?.ToArray().Count(x => x.ObjectType == GameInfoPlayerKillsDeathsType.Death) ?? 0;

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
                WebApiUserId = info?.SearchPlayer?.FirstOrDefault(x => x?.Name == newUsername)?.Id;

                await AddPlayerKillsDeathsToListAsync(await ApiController.GetGameInfoPlayerKillsDeathsFromJsonAsync(WebApiUserId, GameInfoPlayersType.Deaths), GameInfoPlayerKillsDeathsType.Death);
                await AddPlayerKillsDeathsToListAsync(await ApiController.GetGameInfoPlayerTopKillsFromJsonAsync(WebApiUserId, UnitOfTime.Month), GameInfoPlayerKillsDeathsType.Kill);
                await AddPlayerKillsDeathsToListAsync(await ApiController.GetGameInfoPlayerSoloKillsFromJsonAsync(WebApiUserId, UnitOfTime.Month), GameInfoPlayerKillsDeathsType.SoloKill);
                await SaveDungeonsInFileAsync();

                LastUpdate = DateTime.UtcNow;
            }
        }

        private async Task AddPlayerKillsDeathsToListAsync(IEnumerable<GameInfoPlayerKillsDeaths> items, GameInfoPlayerKillsDeathsType type = GameInfoPlayerKillsDeathsType.Unknown)
        {
            PlayerKillsDeaths ??= await LoadDungeonFromFileAsync();

            var playerData = items?.Select(x => new GameInfoPlayerKillsDeathsWithType()
            {
                ObjectType = type,
                GroupMemberCount = x.GroupMemberCount,
                NumberOfParticipants = x.NumberOfParticipants,
                EventId = x.EventId,
                TimeStamp = x.TimeStamp,
                Version = x.Version,
                Killer = x.Killer,
                Victim = x.Victim,
                TotalVictimKillFame = x.TotalVictimKillFame,
                Location = x.Location,
                GvGMatch = x.GvGMatch,
                BattleId = x.BattleId,
                KillArea = x.KillArea,
                Category = x.Category,
                Type = x.Type
            });

            if (playerData == null)
            {
                return;
            }

            foreach (var data in playerData)
            {
                if (PlayerKillsDeaths.Any(x => x.Compare(data)))
                {
                    continue;
                }

                PlayerKillsDeaths.Add(data);
            }
        }

        private async Task<ObservableCollection<GameInfoPlayerKillsDeathsWithType>> LoadDungeonFromFileAsync()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.PlayerKillsDeathsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var localItemString = await File.ReadAllTextAsync(localFilePath);
                    return JsonSerializer.Deserialize<ObservableCollection<GameInfoPlayerKillsDeathsWithType>>(localItemString) ?? new ObservableCollection<GameInfoPlayerKillsDeathsWithType>();
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
            }

            return new ObservableCollection<GameInfoPlayerKillsDeathsWithType>();
        }

        private async Task SaveDungeonsInFileAsync()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.PlayerKillsDeathsFileName}";

            try
            {
                var fileString = JsonSerializer.Serialize(PlayerKillsDeaths);
                await File.WriteAllTextAsync(localFilePath, fileString);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}