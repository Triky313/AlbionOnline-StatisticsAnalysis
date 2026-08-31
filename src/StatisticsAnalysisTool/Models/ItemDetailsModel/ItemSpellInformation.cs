using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Models.ItemDetailsModel;

public sealed class ItemSpellInformation
{
    private static readonly Regex AlbionFormattingTagRegex = new(@"\[(?:/?[a-z]+|[0-9a-f]{6}|-)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DescriptionMarkupTagRegex = new(@"\[(?<closing>/)?(?<type>dmg|heal|cc|debuff|buff|mobility|other)\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex DescriptionFormattingTagRegex = new(@"\[(?:(?<closing>/)?(?<tag>dmg|heal|cc|debuff|buff|mobility|other|b|c)|(?<color>[0-9a-f]{6})|(?<reset>-))\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex UnresolvedPlaceholderRegex = new(@"\{\d+\}", RegexOptions.Compiled);

    public ItemSpellInformation(string uniqueName)
    {
        UniqueName = uniqueName;
        Name = string.Equals(uniqueName, "AUTO_ATTACK", StringComparison.Ordinal)
            ? LocalizationController.Translation("AUTO_ATTACK")
            : SpellData.GetLocalizationName(uniqueName);

        var spell = SpellData.GetSpellByUniqueName(uniqueName);
        var localizedDescription = SpellData.GetLocalizationDescription(uniqueName);
        Description = string.Equals(localizedDescription, uniqueName, StringComparison.Ordinal) ? string.Empty : GetPlainDescription(localizedDescription);
        DescriptionSegments = string.Equals(localizedDescription, uniqueName, StringComparison.Ordinal) ? [] : CreateDescriptionSegments(localizedDescription);
        Types = CreateTypes(spell, localizedDescription);
        Stats = CreateStats(spell);
    }

    public string UniqueName { get; }
    public string Name { get; }
    public IReadOnlyList<ItemSpellDescriptionSegment> DescriptionSegments { get; }
    public string Description { get; }
    public IReadOnlyList<ItemSpellType> Types { get; }
    public IReadOnlyList<ItemSpellStat> Stats { get; }
    public Visibility DescriptionVisibility => DescriptionSegments.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    public Visibility StatsVisibility => Stats.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => ImageController.GetSpellImage(SpellData.GetIconUniqueName(UniqueName)));

    private static IReadOnlyList<ItemSpellType> CreateTypes(GameFileDataSpell spell, string localizedDescription)
    {
        var typeKeys = new List<string>();
        AddType(typeKeys, spell.UiType);
        AddType(typeKeys, spell.Category);

        if (!string.IsNullOrWhiteSpace(localizedDescription))
        {
            foreach (Match match in DescriptionMarkupTagRegex.Matches(localizedDescription))
            {
                if (!match.Groups["closing"].Success && !string.Equals(match.Groups["type"].Value, "other", StringComparison.OrdinalIgnoreCase))
                {
                    AddType(typeKeys, match.Groups["type"].Value);
                }
            }
        }

        if (typeKeys.Count == 0)
        {
            AddType(typeKeys, string.Equals(spell.SpellKind, "passivespell", StringComparison.OrdinalIgnoreCase) ? "passive" : "active");
        }

        return typeKeys.Select(typeKey => new ItemSpellType(typeKey, GetLocalizedTypeName(typeKey))).ToArray();
    }

    private static IReadOnlyList<ItemSpellStat> CreateStats(GameFileDataSpell spell)
    {
        var stats = new List<ItemSpellStat>();
        AddStat(stats, spell, "energyusage", "Energy Cost", spell.EnergyUsage, string.Empty);
        AddStat(stats, spell, "castingtime", "Cast Time", spell.CastingTime, "s");
        AddStat(stats, spell, "channeling[0].totalduration", "Channel Time", spell.ChannelingTime, "s");
        AddStat(stats, spell, "castrange", "Range", spell.CastRange, "m");
        AddStat(stats, spell, "recastdelay", "Cooldown", spell.RecastDelay, "s");
        return stats;
    }

    private static void AddType(ICollection<string> typeKeys, string rawType)
    {
        var normalizedType = NormalizeType(rawType);
        if (!string.IsNullOrWhiteSpace(normalizedType)
            && !typeKeys.Contains(normalizedType, StringComparer.Ordinal))
        {
            typeKeys.Add(normalizedType);
        }
    }

    private static string NormalizeType(string rawType)
    {
        return rawType?.Trim().ToLowerInvariant() switch
        {
            "heal" or "healing" => "heal",
            "dmg" or "damage" => "damage",
            "buff" => "buff",
            "debuff" => "debuff",
            "cc" or "crowdcontrol" => "crowdcontrol",
            "movement" or "mobility" => "movement",
            "passive" => "passive",
            "active" => "active",
            _ => string.Empty
        };
    }

    private static string GetLocalizedTypeName(string typeKey)
    {
        var (localizationKey, fallbackName) = typeKey switch
        {
            "heal" => ("@INPUT_SYMBOLS_SPELL_HEAL", "Healing"),
            "damage" => ("@INPUT_SYMBOLS_SPELL_DAMAGE", "Damage"),
            "buff" => ("@INPUT_SYMBOLS_SPELL_BUFF", "Buff"),
            "debuff" => ("@INPUT_SYMBOLS_SPELL_DEBUFF", "Debuff"),
            "crowdcontrol" => ("@INPUT_SYMBOLS_SPELL_CC", "Crowd Control"),
            "movement" => ("@INPUT_SYMBOLS_SPELL_MOBILITY", "Mobility"),
            "passive" => ("@ITEMDETAILS_STATS_INFO_PASSIVE", "Passive"),
            _ => ("@OPTIONS_GENERIC_ACTIVE", "Active")
        };
        var localizedName = LocalizationController.GameTranslation(localizationKey).Trim('(', ')');
        return string.Equals(localizedName, localizationKey, StringComparison.Ordinal) ? fallbackName : localizedName;
    }

    private static void AddStat(ICollection<ItemSpellStat> stats, GameFileDataSpell spell, string statToken, string fallbackName, string rawValue, string unit)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return;
        }

        var name = GetLocalizedStatName(spell.StatBlockLocatag, statToken, fallbackName);
        stats.Add(new ItemSpellStat(name, FormatValue(rawValue, unit)));
    }

    private static string GetLocalizedStatName(string statBlockLocatag, string statToken, string fallbackName)
    {
        var localizationKey = string.IsNullOrWhiteSpace(statBlockLocatag) ? "@SPELLS_STANDARD_STATBLOCK" : statBlockLocatag;
        var statBlock = LocalizationController.GameTranslation(localizationKey);
        var line = statBlock.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(value => value.Contains($"${statToken}$", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(line))
        {
            return fallbackName;
        }

        var separatorIndex = line.IndexOf('\u00A7');
        var name = separatorIndex >= 0 ? line[..separatorIndex] : line;
        return AlbionFormattingTagRegex.Replace(name, string.Empty).Trim().TrimEnd(':');
    }

    private static string FormatValue(string rawValue, string unit)
    {
        var value = double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ? number.ToString("0.##", CultureInfo.CurrentCulture) : rawValue;
        return value + unit;
    }

    private static IReadOnlyList<ItemSpellDescriptionSegment> CreateDescriptionSegments(string description)
    {
        var normalizedDescription = UnresolvedPlaceholderRegex.Replace(description, "-").Replace("\r\n", "\n", StringComparison.Ordinal);
        var segments = new List<ItemSpellDescriptionSegment>();
        var activeTypes = new Stack<string>();
        var activeColors = new Stack<string>();
        var activeColor = string.Empty;
        var boldDepth = 0;
        var currentPosition = 0;

        foreach (Match match in DescriptionFormattingTagRegex.Matches(normalizedDescription))
        {
            AddDescriptionSegment(segments, normalizedDescription[currentPosition..match.Index], activeTypes.Count > 0 ? activeTypes.Peek() : string.Empty, boldDepth > 0 || activeTypes.Count > 0, activeColor);

            if (match.Groups["color"].Success)
            {
                activeColor = match.Groups["color"].Value;
            }
            else if (match.Groups["reset"].Success)
            {
                activeTypes.Clear();
                activeColors.Clear();
                activeColor = string.Empty;
                boldDepth = 0;
            }
            else
            {
                var tag = match.Groups["tag"].Value.ToLowerInvariant();
                var isClosingTag = match.Groups["closing"].Success;
                if (tag == "b")
                {
                    boldDepth = isClosingTag ? Math.Max(0, boldDepth - 1) : boldDepth + 1;
                }
                else if (tag == "c")
                {
                    if (isClosingTag)
                    {
                        activeColor = activeColors.Count > 0 ? activeColors.Pop() : string.Empty;
                    }
                    else
                    {
                        activeColors.Push(activeColor);
                    }
                }
                else if (isClosingTag)
                {
                    if (activeTypes.Count > 0)
                    {
                        activeTypes.Pop();
                    }
                }
                else
                {
                    activeTypes.Push(NormalizeDescriptionType(tag));
                }
            }

            currentPosition = match.Index + match.Length;
        }

        AddDescriptionSegment(segments, normalizedDescription[currentPosition..], activeTypes.Count > 0 ? activeTypes.Peek() : string.Empty, boldDepth > 0 || activeTypes.Count > 0, activeColor);
        return segments;
    }

    private static void AddDescriptionSegment(ICollection<ItemSpellDescriptionSegment> segments, string text, string typeKey, bool isBold, string colorHex)
    {
        var plainText = AlbionFormattingTagRegex.Replace(text, string.Empty);
        if (plainText.Length > 0)
        {
            segments.Add(new ItemSpellDescriptionSegment(plainText, typeKey, isBold, colorHex));
        }
    }

    private static string NormalizeDescriptionType(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "dmg" => "damage",
            "cc" => "crowdcontrol",
            "mobility" => "movement",
            _ => type.ToLowerInvariant()
        };
    }

    private static string GetPlainDescription(string description)
    {
        var plainDescription = AlbionFormattingTagRegex.Replace(description, string.Empty);
        return UnresolvedPlaceholderRegex.Replace(plainDescription, "-")
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Trim();
    }
}