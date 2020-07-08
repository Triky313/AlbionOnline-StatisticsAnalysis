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
                ? ItemController.LocalizedName(LocalizedNames, null, UniqueName)
                : $"{ItemController.LocalizedName(LocalizedNames, null, UniqueName)}\n{ItemController.LocalizedName(LocalizedNames, "EN-US", string.Empty)}";

        public int Level => ItemController.GetItemLevel(UniqueName);
        public int Tier => ItemController.GetItemTier(this);

        private BitmapImage _icon;
        public BitmapImage Icon => _icon ?? (_icon = ImageController.GetItemImage($"https://render.albiononline.com/v1/item/{UniqueName}.png"));

        public BitmapImage ExistFullItemInformationLocal => ItemController.ExistFullItemInformationLocal(UniqueName);
        public ItemInformation FullItemInformation { get; set; }
    }
}
