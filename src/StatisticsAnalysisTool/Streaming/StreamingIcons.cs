using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace StatisticsAnalysisTool.Streaming;

/// <summary>Serves the tool's own icons to the stream page. Only names on this list can be requested.</summary>
public static class StreamingIcons
{
    private static readonly Dictionary<string, string> Paths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["fame"] = "Resources/fame.png",
        ["silver"] = "Resources/silver.png",
        ["respec"] = "Resources/respec.png",
        ["might"] = "Resources/might.png",
        ["favor"] = "Resources/favor.png",
        ["loot"] = "Assets/bag.png",
        ["gathering"] = "Assets/pickaxe_box.png",
        ["wood"] = "Assets/MiniMapMarker/minimapmarker_wood.png",
        ["ore"] = "Assets/MiniMapMarker/minimapmarker_ore.png",
        ["fiber"] = "Assets/MiniMapMarker/minimapmarker_fiber.png",
        ["hide"] = "Assets/MiniMapMarker/minimapmarker_hide.png",
        ["rock"] = "Assets/MiniMapMarker/minimapmarker_stone.png",
        ["fish"] = "Assets/fishing_lure_box.png",
        ["faction-martlock"] = "Resources/factioncoin_martlock.png",
        ["faction-lymhurst"] = "Resources/factioncoin_lymhurst.png",
        ["faction-fortsterling"] = "Resources/factioncoin_fortsterling.png",
        ["faction-bridgewatch"] = "Resources/factioncoin_bridgewatch.png",
        ["faction-thetford"] = "Resources/factioncoin_thetford.png",
        ["faction-caerleon"] = "Resources/factioncoin_caerleon.png"
    };

    private static readonly ConcurrentDictionary<string, byte[]> Cache = new(StringComparer.OrdinalIgnoreCase);

    public static byte[] Get(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        name = name.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ? name[..^4] : name;
        if (!Paths.TryGetValue(name, out var path))
        {
            return null;
        }

        return Cache.GetOrAdd(name, _ => Load(path));
    }

    private static byte[] Load(string path)
    {
        try
        {
            var info = Application.GetResourceStream(new Uri($"pack://application:,,,/{path}"));
            if (info?.Stream == null)
            {
                return null;
            }

            using var stream = info.Stream;
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Streaming icon {Path} could not be loaded", path);
            return null;
        }
    }
}
