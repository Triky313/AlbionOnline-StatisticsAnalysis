using StatisticsAnalysisTool.Common;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models
{
    public class Item
    {
        public string LocalizationNameVariable { get; set; }
        public string LocalizationDescriptionVariable { get; set; }
        public LocalizedNames LocalizedNames { get; set; }
        public int Index { get; set; }
        public string UniqueName { get; set; }
        
        public string LocalizedNameAndEnglish =>
            LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper() == "EN-US"
                ? ItemController.LocalizedName(LocalizedNames)
                : $"{ItemController.LocalizedName(LocalizedNames)}\n{ItemController.LocalizedName(LocalizedNames, "EN-US")}";

        public int Level => ItemController.GetItemLevel(UniqueName);
        public int Tier => ItemController.GetItemTier(this);

        private BitmapImage _icon;
        public BitmapImage Icon => _icon ?? (_icon = ImageController.GetItemImage($"https://gameinfo.albiononline.com/api/gameinfo/items/{UniqueName}"));

        private BitmapImage _existFullItemInformationLocal;
        public BitmapImage ExistFullItemInformationLocal => _existFullItemInformationLocal ?? (_existFullItemInformationLocal = ItemController.ExistFullItemInformationLocal(UniqueName));
        public ItemInformation FullItemInformation { get; set; }
    }
}
