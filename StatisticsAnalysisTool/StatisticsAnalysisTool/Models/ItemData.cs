using System.Collections.Generic;
using System.Text;
using StatisticsAnalysisTool.Common;
using Newtonsoft.Json.Linq;

namespace StatisticsAnalysisTool.Models
{
    public class ItemData
    {
        public string ItemType { get; set; }
        public string UniqueName { get; set; }
        public string UiSprite { get; set; }
        public string UiAtlas { get; set; }
        public bool Showinmarketplace { get; set; }
        public int Level { get; set; }
        public int Tier { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Revision { get; set; }
        public string Enchantments { get; set; }
        public List<string> ActiveSlots { get; set; }
        public List<string> PassiveSlots { get; set; }
        //public List<KeyValueStruct> LocalizedNames { get; set; }
        public List<KeyValueStruct> LocalizedNames { get; set; }
        public string LocalizedName
        {
            get
            {
                var name = UniqueName;

                if (LocalizedNames.Exists(a => a.Key == LanguageController.CurrentLanguage.ToUpper()))
                {
                    name = LocalizedNames.Find(a => a.Key == LanguageController.CurrentLanguage.ToUpper()).Value;
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(name));
                }

                if (LocalizedNames.Exists(a => a.Key == LanguageController.DefaultCultureInfo.Name.ToUpper()))
                {
                    name = LocalizedNames.Find(a => a.Key == LanguageController.DefaultCultureInfo.Name.ToUpper()).Value;
                    return Encoding.UTF8.GetString(Encoding.Default.GetBytes(name));
                }

                return name;
            }
        }
        //public List<KeyValueStruct> LocalizedDescriptions { get; set; }
        public string LocalizedDescriptions { get; set; }
        public string SlotType { get; set; }
        public int CraftFameGainFactor { get; set; }
        public int PlaceCost { get; set; }
        public int PlaceFame { get; set; }
        public bool Pickupable { get; set; }
        public bool Destroyable { get; set; }
        public bool UnlockedToPlace { get; set; }
        public string SpriteName { get; set; }
        public bool Stackable { get; set; }
        public bool Equipable { get; set; }
        
        public struct KeyValueStruct
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

    }
}
