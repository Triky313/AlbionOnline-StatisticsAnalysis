namespace StatisticsAnalysisTool.Models.ApiModel;

public class Equipment
{
    public MainHand MainHand { get; set; }
    public object OffHand { get; set; }
    public Head Head { get; set; }
    public Armor Armor { get; set; }
    public Shoes Shoes { get; set; }
    public Bag Bag { get; set; }
    public Cape Cape { get; set; }
    public Mount Mount { get; set; }
    public Potion Potion { get; set; }
    public Food Food { get; set; }
}