using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class OtherGrabbedLootNotificationFragment : LineFragment
    {
        public OtherGrabbedLootNotificationFragment(string looter, string lootedPlayer, Item item, int quantity)
        {
            Looter = looter;
            LootedPlayer = lootedPlayer;
            Item = item;
            Quantity = quantity;
        }
        
        public string Looter { get; }
        public string LootedTranslation => LanguageController.Translation("LOOTED");
        public Item Item { get; }
        public int Quantity { get; }
        public string FromTranslation => LanguageController.Translation("FROM");
        public string LootedPlayer { get; }
    }
}