using StatisticsAnalysisTool.Models;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingItemSearchResult
{
    public string Name { get; set; }
    public BitmapImage Icon { get; set; }
    public Item Value { get; set; }
}