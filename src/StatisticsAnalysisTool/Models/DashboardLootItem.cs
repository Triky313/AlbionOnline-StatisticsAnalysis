using System;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardLootItem
{
    public DashboardLootItem(Item item, int quantity, double totalValue, DateTime occurredAtUtc, bool displayUnitValue = false)
    {
        Item = item;
        Quantity = quantity;
        TotalValue = totalValue;
        UnitValue = quantity > 0 ? totalValue / quantity : 0;
        DisplayValue = displayUnitValue ? UnitValue : totalValue;
        ShowQuantity = !displayUnitValue;
        LootedAtLocal = occurredAtUtc.ToLocalTime();
    }

    public Item Item { get; }
    public int Quantity { get; }
    public double TotalValue { get; }
    public double UnitValue { get; }
    public double DisplayValue { get; }
    public bool ShowQuantity { get; }
    public DateTime LootedAtLocal { get; }
}