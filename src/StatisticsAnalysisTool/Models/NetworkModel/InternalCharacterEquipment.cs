using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class InternalCharacterEquipment
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
}