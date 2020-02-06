using Newtonsoft.Json.Linq;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System;
using System.Linq;
using System.Text;

namespace StatisticsAnalysisTool
{
    public class StatisticsAnalysisManager
    {
        public static string LocalizedName(ItemInformation itemInformation, string uniqueName)
        {

            switch (LanguageController.CurrentLanguage.ToUpper())
            {
                case LanguageController.LocalizedNamesDictionary:

                    break;
                default:
            }

            if (itemInformation.LocalizedNames.Exists(a => a.Key == LanguageController.CurrentLanguage.ToUpper()))
            {
                uniqueName = itemInformation.LocalizedNames.Find(a => a.Key == LanguageController.CurrentLanguage.ToUpper()).Value;
                return Encoding.UTF8.GetString(Encoding.Default.GetBytes(uniqueName));
            }

            if (itemInformation.LocalizedNames.Exists(a => a.Key == LanguageController.DefaultCultureInfo.Name.ToUpper()))
            {
                uniqueName = itemInformation.LocalizedNames.Find(a => a.Key == LanguageController.DefaultCultureInfo.Name.ToUpper()).Value;
                return Encoding.UTF8.GetString(Encoding.Default.GetBytes(uniqueName));
            }

            return name;
        }

        public static FrequentlyValues.ItemTier GetItemTier(string uniqueName) => FrequentlyValues.ItemTiers.FirstOrDefault(x => x.Value == uniqueName.Split('_')[0]).Key;

        public static FrequentlyValues.ItemLevel GetItemLevel(string uniqueName)
        {
            if (!uniqueName.Contains("@"))
                return FrequentlyValues.ItemLevel.Level0;

            if(int.TryParse(uniqueName.Split('@')[1], out int number))
                return FrequentlyValues.ItemLevels.First(x => x.Value == number).Key;
            return FrequentlyValues.ItemLevel.Level0;
        }

        public static int GetQuality(FrequentlyValues.ItemQuality value) => FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Key == value).Value;

        public static FrequentlyValues.ItemQuality GetQuality(int value) => FrequentlyValues.ItemQualities.FirstOrDefault(x => x.Value == value).Key;

        public static void AddLocalizedName(ref ItemData itemData, JObject parsedObject)
        {
            foreach (var language in Enum.GetValues(typeof(FrequentlyValues.GameLanguage)).Cast<FrequentlyValues.GameLanguage>())
            {
                var cultureCode = FrequentlyValues.GameLanguages.FirstOrDefault(x => x.Key == language).Value;

                if (parsedObject["localizedNames"]?[cultureCode] != null)
                    itemData.LocalizedNames.Add(new ItemData.KeyValueStruct() { Key = cultureCode, Value = parsedObject["localizedNames"][cultureCode].ToString() });
            }
        }
    }
}
