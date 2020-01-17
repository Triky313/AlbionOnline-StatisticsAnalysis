using System.Collections.Generic;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Common
{
    public static class FrequentlyValues
    {
        public enum GameLanguage { UnitedStates, Germany, Russia, Poland, Brazil, France, Spain }

        public enum ItemTier { T1 = 0, T2 = 1, T3 = 2, T4 = 3, T5 = 4, T6 = 5, T7 = 6, T8 = 7 }

        public enum ItemLevel { Level0 = 0, Level1 = 1, Level2 = 2, Level3 = 3 }
        
        public enum ItemQuality { Normal = 0, Good = 1, Outstanding = 2, Excellent = 3, Masterpiece = 4 }
        
        public static readonly Dictionary<ItemTier, string> ItemTiers = new Dictionary<ItemTier, string>
        {
            {ItemTier.T1, "T1" },
            {ItemTier.T2, "T2" },
            {ItemTier.T3, "T3" },
            {ItemTier.T4, "T4" },
            {ItemTier.T5, "T5" },
            {ItemTier.T6, "T6" },
            {ItemTier.T7, "T7" },
            {ItemTier.T8, "T8" }
        };

        public static readonly Dictionary<ItemLevel, int> ItemLevels = new Dictionary<ItemLevel, int>
        {
            {ItemLevel.Level0, 0 },
            {ItemLevel.Level1, 1 },
            {ItemLevel.Level2, 2 },
            {ItemLevel.Level3, 3 }
        };

        public static readonly Dictionary<ItemQuality, int> ItemQualities = new Dictionary<ItemQuality, int>
        {
            {ItemQuality.Normal, 1 },
            {ItemQuality.Good, 2 },
            {ItemQuality.Outstanding, 3 },
            {ItemQuality.Excellent, 4 },
            {ItemQuality.Masterpiece, 5 }
        };

        public static readonly Dictionary<GameLanguage, string> GameLanguages = new Dictionary<GameLanguage, string>()
        {
            {GameLanguage.UnitedStates, "EN-US" },
            {GameLanguage.Germany, "DE-DE" },
            {GameLanguage.Russia, "RU-RU" },
            {GameLanguage.Poland, "PL-PL" },
            {GameLanguage.Brazil, "PT-BR" },
            {GameLanguage.France, "FR-FR" },
            {GameLanguage.Spain, "ES-ES" }
        };
    }
}