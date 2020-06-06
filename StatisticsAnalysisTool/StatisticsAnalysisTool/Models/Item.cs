using Newtonsoft.Json;
using StatisticsAnalysisTool.Common;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models
{
    public class Item
    {
        [JsonProperty("LocalizationNameVariable")]
        public string LocalizationNameVariable { get; set; }
        [JsonProperty("LocalizationDescriptionVariable")]
        public string LocalizationDescriptionVariable { get; set; }
        [JsonProperty("LocalizedNames")]
        public LocalizedNames LocalizedNames { get; set; }
        //public List<KeyValueStruct> LocalizedDescriptions { get; set; }
        [JsonProperty("Index")]
        public int Index { get; set; }
        [JsonProperty("UniqueName")]
        public string UniqueName { get; set; }

        public string LocalizedNameAndEnglish =>
            LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper() == "EN-US"
                ? ItemController.LocalizedName(LocalizedNames)
                : $"{ItemController.LocalizedName(LocalizedNames)}\n{ItemController.LocalizedName(LocalizedNames, "EN-US")}";

        private BitmapImage _icon;
        public BitmapImage Icon => _icon ?? (_icon = ImageController.GetItemImage($"https://gameinfo.albiononline.com/api/gameinfo/items/{UniqueName}"));

        private ItemInformation _fullItemInformation;

        public ItemInformation FullItemInformation
        {
            get
            {
                var test = _fullItemInformation ?? (_fullItemInformation = ItemController.GetItemInformationAsync(UniqueName).Result);
                return test;
            }
        }
        public bool IsFullItemInformationUpToDate => ItemController.IsItemInformationUpToDate(_fullItemInformation?.LastUpdate);
    }
}
