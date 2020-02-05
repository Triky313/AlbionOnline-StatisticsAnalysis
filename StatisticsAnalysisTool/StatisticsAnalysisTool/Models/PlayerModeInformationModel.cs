using System;

namespace StatisticsAnalysisTool.Models
{
    public class PlayerModeInformationModel
    {
        public DateTime Timestamp { get; set; }
        public GameInfoSearchResponse GameInfoSearch { get; set; }
        public SearchPlayerResponse SearchPlayer { get; set; }
        public GameInfoPlayersResponse GameInfoPlayers { get; set; }
    }
}