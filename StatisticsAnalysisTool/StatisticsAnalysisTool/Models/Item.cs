using FontAwesome.WPF;
using StatisticsAnalysisTool.Common;
using System.Windows.Media;
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

        public string LocalizedNameAndEnglish => LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper() == "EN-US"
            ? ItemController.LocalizedName(LocalizedNames, null, UniqueName)
            : $"{ItemController.LocalizedName(LocalizedNames, null, UniqueName)}\n{ItemController.LocalizedName(LocalizedNames, "EN-US", string.Empty)}{GetUniqueNameIfDebug()}";

        private string GetUniqueNameIfDebug()
        {
#if DEBUG
            return $"\n{UniqueName}";
#else
            return string.Empty;
#endif
        }

        public string LocalizedName => ItemController.LocalizedName(LocalizedNames, null, UniqueName);

        public int Level => ItemController.GetItemLevel(UniqueName);
        public int Tier => ItemController.GetItemTier(this);

        private BitmapImage _icon;
        public BitmapImage Icon => _icon ?? (_icon = ImageController.GetItemImage(UniqueName));

        public BitmapImage ExistFullItemInformationLocal => ItemController.ExistFullItemInformationLocal(UniqueName);
        public ItemInformation FullItemInformation { get; set; }

        public int AlertModeMinSellPriceIsUndercutPrice { get; set; }
        public bool IsAlertActive { get; set; }
        public FontAwesomeIcon AlertToggle => (IsAlertActive) ? FontAwesomeIcon.ToggleOn : FontAwesomeIcon.ToggleOff;
        public Brush AlertToggleColor => (IsAlertActive) ? ItemController.AlertToggleOnColor : ItemController.AlertToggleOffColor;
    }
}