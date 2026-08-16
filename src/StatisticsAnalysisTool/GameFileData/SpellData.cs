using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StatisticsAnalysisTool.GameFileData;

public static class SpellData
{
    private static readonly Regex InlineLocalizationReferenceRegex = new(@"\$(?<reference>\$?[^$]+)\$", RegexOptions.Compiled);
    private static List<GameFileDataSpell> _spells;
    private static IReadOnlyDictionary<string, XElement> _spellElementsByUniqueName =
        new Dictionary<string, XElement>(StringComparer.Ordinal);

    public static string GetUniqueName(int index)
    {
        return GetSpellByIndex(index)?.UniqueName ?? string.Empty;
    }

    public static bool IsDataLoaded()
    {
        return _spells?.Count > 0;
    }

    public static GameFileDataSpell GetSpellByIndex(int index)
    {
        if (!IsDataLoaded())
        {
            return new GameFileDataSpell();
        }

        return _spells.IsInBounds(index) ? _spells[index] : new GameFileDataSpell();
    }

    public static GameFileDataSpell GetSpellByUniqueName(string uniqueName)
    {
        if (!IsDataLoaded() || string.IsNullOrWhiteSpace(uniqueName))
        {
            return new GameFileDataSpell();
        }

        return _spells.FirstOrDefault(x => x.UniqueName == uniqueName) ?? new GameFileDataSpell();
    }

    public static string GetLocalizationName(string uniqueName)
    {
        var spell = GetSpellByUniqueName(uniqueName);
        if (string.IsNullOrWhiteSpace(spell.UniqueName))
        {
            return uniqueName;
        }

        var localizationKey = string.IsNullOrWhiteSpace(spell.NameLocatag)
            ? $"@SPELLS_{uniqueName}"
            : spell.NameLocatag;
        var localizedName = LocalizationController.GameTranslation(localizationKey);
        return string.Equals(localizedName, localizationKey, StringComparison.Ordinal) ? uniqueName : localizedName;
    }

    public static string GetLocalizationDescription(string uniqueName)
    {
        var spell = GetSpellByUniqueName(uniqueName);
        if (string.IsNullOrWhiteSpace(spell.UniqueName))
        {
            return uniqueName;
        }

        var localizationKey = GetFirstExistingLocalizationKey(
            spell.DescriptionLocatag,
            $"@SPELLS_{uniqueName}_V2_DESC",
            $"@SPELLS_{uniqueName}_DESC");
        if (string.IsNullOrWhiteSpace(localizationKey))
        {
            return uniqueName;
        }

        var placeholders = Enumerable.Range(0, spell.DescriptionValues.Count)
            .Select(index => index.ToString(CultureInfo.InvariantCulture))
            .ToList();
        var localizedDescription = LocalizationController.Translation(
            localizationKey,
            placeholders,
            spell.DescriptionValues.ToList());
        return ResolveInlineLocalizationReferences(spell, localizedDescription);
    }

    public static async Task<bool> LoadDataAsync()
    {
        var gameFilesDirPath = AppDataPaths.GameFilesDirectory;
        var regularDataFilePath = Path.Combine(gameFilesDirPath, Settings.Default.SpellDataFileName);

        if (!File.Exists(regularDataFilePath))
        {
            _spells = new List<GameFileDataSpell>();
            _spellElementsByUniqueName = new Dictionary<string, XElement>(StringComparer.Ordinal);
            return false;
        }

        _spells = await Task.Run(() =>
        {
            var document = XDocument.Load(regularDataFilePath);
            return BuildSpells([.. document.Root!.Elements()]);
        }).ConfigureAwait(false);

        return _spells.Count >= 0;
    }

    public static List<GameFileDataSpell> BuildSpells(List<XElement> elements)
    {
        var spellElementsByUniqueName = elements
            .Where(element => element.Name != "colortag" && element.Attribute("uniquename") != null)
            .GroupBy(element => element.Attribute("uniquename")!.Value, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

        _spellElementsByUniqueName = spellElementsByUniqueName;
        var spells = new List<GameFileDataSpell>();
        var index = 0;

        foreach (var element in elements)
        {
            if (element.Name == "colortag")
            {
                // skip
            }
            else if (element.Name == "passivespell")
            {
                var passiveSpell = CreateGameFileDataSpell(index++, element, spellElementsByUniqueName);
                if (passiveSpell != null)
                {
                    spells.Add(passiveSpell);
                }
            }
            else if (element.Name == "activespell")
            {
                var activeSpell = CreateGameFileDataSpell(index++, element, spellElementsByUniqueName);
                if (activeSpell != null)
                {
                    spells.Add(activeSpell);
                }

                if (element.Element("channelingspell") != null)
                {
                    var channelingSpell = CreateGameFileDataSpell(index++, element, spellElementsByUniqueName);
                    if (channelingSpell != null)
                    {
                        spells.Add(channelingSpell);
                    }
                }
            }
            else if (element.Name == "togglespell")
            {
                var toggleSpell = CreateGameFileDataSpell(index++, element, spellElementsByUniqueName);
                if (toggleSpell != null)
                {
                    spells.Add(toggleSpell);
                }
            }
            else
            {
                throw new FormatException();
            }
        }

        return spells;
    }

    private static GameFileDataSpell CreateGameFileDataSpell(
        int index,
        XElement element,
        IReadOnlyDictionary<string, XElement> spellElementsByUniqueName)
    {
        var uniqueName = element.Attribute("uniquename")?.Value ?? string.Empty;
        var nameLocatag = element.Attribute("namelocatag")?.Value ?? string.Empty;
        var descriptionLocatag = element.Attribute("descriptionlocatag")?.Value ?? string.Empty;
        var target = element.Attribute("target")?.Value ?? string.Empty;
        var category = element.Attribute("category")?.Value ?? string.Empty;
        var channelingTime = SpellLocalizationReferenceResolver.GetChannelingDuration(element.Element("channelingspell"));
        var descriptionValues = GetDescriptionValues(element, spellElementsByUniqueName);

        if (!string.IsNullOrEmpty(uniqueName))
        {
            return new GameFileDataSpell()
            {
                Index = index,
                UniqueName = uniqueName,
                Target = target,
                Category = category,
                NameLocatag = nameLocatag,
                DescriptionLocatag = descriptionLocatag,
                SpellKind = element.Name.LocalName,
                UiType = element.Attribute("uitype")?.Value ?? string.Empty,
                EnergyUsage = element.Attribute("energyusage")?.Value ?? string.Empty,
                CastingTime = element.Attribute("castingtime")?.Value ?? string.Empty,
                RecastDelay = element.Attribute("recastdelay")?.Value ?? string.Empty,
                CastRange = element.Attribute("castrange")?.Value ?? string.Empty,
                ChannelingTime = channelingTime,
                StatBlockLocatag = element.Attribute("statblock")?.Value ?? string.Empty,
                DescriptionValues = descriptionValues
            };
        }

        return null;
    }

    private static IReadOnlyList<string> GetDescriptionValues(
        XElement spellElement,
        IReadOnlyDictionary<string, XElement> spellElementsByUniqueName)
    {
        var references = spellElement.Element("locareferences")?
            .Element("description")?
            .Elements("locareference") ?? [];
        var spellUniqueName = spellElement.Attribute("uniquename")?.Value ?? string.Empty;
        return references
            .Select(reference => SpellLocalizationReferenceResolver.Resolve(
                reference.Attribute("tag")?.Value,
                spellUniqueName,
                spellElementsByUniqueName))
            .ToArray();
    }

    private static string ResolveInlineLocalizationReferences(GameFileDataSpell spell, string localizedDescription)
    {
        if (string.IsNullOrWhiteSpace(localizedDescription))
        {
            return localizedDescription;
        }

        return InlineLocalizationReferenceRegex.Replace(localizedDescription, match =>
        {
            var resolvedValue = SpellLocalizationReferenceResolver.Resolve(
                match.Value,
                spell.UniqueName,
                _spellElementsByUniqueName);
            return string.Equals(resolvedValue, "-", StringComparison.Ordinal)
                ? match.Value
                : resolvedValue;
        });
    }

    private static string GetFirstExistingLocalizationKey(params string[] localizationKeys)
    {
        return localizationKeys
            .FirstOrDefault(localizationKey =>
                !string.IsNullOrWhiteSpace(localizationKey)
                && !string.Equals(LocalizationController.GameTranslation(localizationKey), localizationKey, StringComparison.Ordinal))
            ?? string.Empty;
    }
}
