using System.Text;
using System.Windows.Media.Imaging;
using StatisticsAnalysisTool.Common;
using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class Item
    {
        [JsonProperty("LocalizationNameVariable")]
        public string LocalizationNameVariable { get; set; }
        [JsonProperty("LocalizationDescriptionVariable")]
        public string LocalizationDescriptionVariable { get; set; }
        [JsonProperty("LocalizedNames")]
        public LocalizedNamesStruct? LocalizedNames { get; set; }
        //public List<KeyValueStruct> LocalizedDescriptions { get; set; }
        [JsonProperty("Index")]
        public int Index { get; set; }
        [JsonProperty("UniqueName")]
        public string UniqueName { get; set; }
        public string LocalizedName(string language = null)
        {
            var name = UniqueName;
            var lang = LanguageController.CurrentLanguage;

            if (language != null)
                lang = language;

            switch (lang.ToUpper())
            {
                case "EN-US":
                    if (LocalizedNames?.EnUs != "")
                        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.EnUs ?? name));
                    break;
                case "DE-DE":
                    if (LocalizedNames?.DeDe != "")
                        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.DeDe ?? name));
                    break;
                case "RU-RU":
                    if (LocalizedNames?.RuRu != "")
                        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.RuRu ?? name));
                    break;
                case "PL-PL":
                    if (LocalizedNames?.PlPl != "")
                        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.PlPl ?? name));
                    break;
                case "PT-BR":
                    if (LocalizedNames?.PtBr != "")
                        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.PtBr ?? name));
                    break;
                case "FR-FR":
                    if (LocalizedNames?.FrFr != "")
                        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.FrFr ?? name));
                    break;
                case "ES-ES":
                    if (LocalizedNames?.EsEs != "")
                        return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.EsEs ?? name));
                    break;
                default:
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(LocalizedNames?.EnUs ?? name));
            }

            return name;
        }
        public string LocalizedNameAndEnglish => LanguageController.CurrentLanguage.ToUpper() == "EN-US" ? LocalizedName() : $"{LocalizedName()}\n{LocalizedName("EN-US")}";
        private BitmapImage _icon;
        public BitmapImage Icon => _icon ?? (_icon = ImageController.GetItemImage($"https://gameinfo.albiononline.com/api/gameinfo/items/{UniqueName}"));

        public struct LocalizedNamesStruct
        {
            [JsonProperty("EN-US")]
            public string EnUs { get; set; }
            [JsonProperty("DE-DE")]
            public string DeDe { get; set; }
            [JsonProperty("FR-FR")]
            public string FrFr { get; set; }
            [JsonProperty("RU-RU")]
            public string RuRu { get; set; }
            [JsonProperty("PL-PL")]
            public string PlPl { get; set; }
            [JsonProperty("ES-ES")]
            public string EsEs { get; set; }
            [JsonProperty("PT-BR")]
            public string PtBr { get; set; }
        }
    }
}
