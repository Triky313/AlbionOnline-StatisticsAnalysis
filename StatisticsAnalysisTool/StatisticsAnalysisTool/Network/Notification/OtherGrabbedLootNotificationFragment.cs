using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class OtherGrabbedLootNotificationFragment : LineFragment
    {
        public OtherGrabbedLootNotificationFragment(string looter, string lootedPlayer, Item item, int quantity)
        {
            Looter = looter;
            LootedPlayer = lootedPlayer;
            LocalizedName = item.LocalizedName;
            Icon = item.Icon;
            Quantity = quantity;
        }
        
        public string Looter { get; }
        public string LootedTranslation => LanguageController.Translation("LOOTED");
        public string LocalizedName { get; }
        public BitmapImage Icon { get; }
        public int Quantity { get; }
        public string FromTranslation => LanguageController.Translation("FROM");
        public string LootedPlayer { get; }
    }
}