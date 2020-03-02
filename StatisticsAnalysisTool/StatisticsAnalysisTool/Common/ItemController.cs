namespace StatisticsAnalysisTool.Common
{
    using Models;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Text;

    public class ItemController
    {
        public static string LocalizedName(LocalizedNames localizedNames, string currentLanguage = null, string alternativeName = "NO_ITEM_NAME")
        {
            if (localizedNames == null)
                return "";

            if (string.IsNullOrEmpty(currentLanguage))
                currentLanguage = LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper();

            switch (FrequentlyValues.GameLanguages.FirstOrDefault(x => string.Equals(x.Value, currentLanguage, StringComparison.CurrentCultureIgnoreCase)).Key)
            {
                case FrequentlyValues.GameLanguage.UnitedStates:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.EnUs));
                case FrequentlyValues.GameLanguage.Germany:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.DeDe));
                case FrequentlyValues.GameLanguage.Russia:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.RuRu));
                case FrequentlyValues.GameLanguage.Poland:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.PlPl));
                case FrequentlyValues.GameLanguage.Brazil:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.PtBr));
                case FrequentlyValues.GameLanguage.France:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.FrFr));
                case FrequentlyValues.GameLanguage.Spain:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.EsEs));
                case FrequentlyValues.GameLanguage.Chinese:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(localizedNames.ZhCn));
                default:
                    return alternativeName;
            }
        }

        public static FrequentlyValues.ItemTier GetItemTier(string uniqueName) => FrequentlyValues.ItemTiers.FirstOrDefault(x => x.Value == uniqueName.Split('_')[0]).Key;

        public static FrequentlyValues.ItemLevel GetItemLevel(string uniqueName)
        {
            if (!uniqueName.Contains("@"))
                return FrequentlyValues.ItemLevel.Level0;

            if (int.TryParse(uniqueName.Split('@')[1], out int number))
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