using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.ItemsJsonModel;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class CharacterEquipment
{
    public int MainHand { get; set; }
    public int OffHand { get; set; }
    public int Head { get; set; }
    public int Chest { get; set; }
    public int Shoes { get; set; }
    public int Bag { get; set; }
    public int Cape { get; set; }
    public int Mount { get; set; }
    public int Potion { get; set; }
    public int BuffFood { get; set; }
    public List<SlotSpell> ActiveSpells { get; set; } = new();

    public Item GetMainHand() => GetItem(MainHand, SlotType.MainHand);
    public Item GetOffHand() => GetItem(OffHand, SlotType.OffHand);
    public Item GetHead() => GetItem(Head, SlotType.Head);
    public Item GetChest() => GetItem(Chest, SlotType.Armor);
    public Item GetShoes() => GetItem(Shoes, SlotType.Shoes);
    public Item GetBag() => GetItem(Bag, SlotType.Bag);
    public Item GetCape() => GetItem(Cape, SlotType.Cape);
    public Item GetMount() => GetItem(Mount, SlotType.Mount);
    public Item GetPotion() => GetItem(Potion, SlotType.Potion);
    public Item GetBuffFood() => GetItem(BuffFood, SlotType.Food);
    public IEnumerable<Spell> GetMainHandSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.MainHand).Select(x => new Spell(x.Value)).AsEnumerable();
    public IEnumerable<Spell> GetOffHandSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.OffHand).Select(x => new Spell(x.Value)).AsEnumerable();
    public IEnumerable<Spell> GetHeadSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.Head).Select(x => new Spell(x.Value)).AsEnumerable();
    public IEnumerable<Spell> GetChestSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.Armor).Select(x => new Spell(x.Value)).AsEnumerable();
    public IEnumerable<Spell> GetShoesSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.Shoes).Select(x => new Spell(x.Value)).AsEnumerable();
    public IEnumerable<Spell> GetMountSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.Mount).Select(x => new Spell(x.Value)).AsEnumerable();
    public IEnumerable<Spell> GetPotionSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.Potion).Select(x => new Spell(x.Value)).AsEnumerable();
    public IEnumerable<Spell> GetFoodSpells() => ActiveSpells.Where(x => x.SlotType == SlotType.Food).Select(x => new Spell(x.Value)).AsEnumerable();

    private static Item GetItem(int? itemIndex, SlotType returnOnlyThisItemType)
    {
        var item = ItemController.GetItemByIndex(itemIndex);

        return item?.FullItemInformation switch
        {
            Weapon weapon when weapon.SlotTypeEnum == returnOnlyThisItemType => item,
            EquipmentItem equipmentItem when equipmentItem.SlotTypeEnum == returnOnlyThisItemType => item,
            ConsumableItem consumableItem when consumableItem.SlotTypeEnum == returnOnlyThisItemType => item,
            Mount mount when mount.SlotTypeEnum == returnOnlyThisItemType => item,
            _ => null
        };
    }
}