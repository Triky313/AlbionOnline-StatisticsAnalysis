using System;

namespace StatisticsAnalysisTool.Network.Manager;

[Flags]
internal enum DashboardUpdateScope
{
    None = 0,
    Chart = 1 << 0,
    Summary = 1 << 1,
    Combat = 1 << 2,
    Mobs = 1 << 3,
    Loot = 1 << 4,
    LootedChests = 1 << 5,
    ContentRankings = 1 << 6,
    Economy = 1 << 7,
    All = Chart | Summary | Combat | Mobs | Loot | LootedChests | ContentRankings | Economy
}
