using System;

namespace StatisticsAnalysisTool.DamageMeter;

internal readonly record struct DamageStatsEvent(DateTime Timestamp, long Value);
