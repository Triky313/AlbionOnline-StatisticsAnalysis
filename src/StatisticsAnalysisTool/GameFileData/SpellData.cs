using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData.Models;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StatisticsAnalysisTool.GameFileData;

public static class SpellData
{
    private static List<GameFileDataSpell> _spells;

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

        if (index < 0)
        {
            uint unsignedIndex = Convert.ToUInt32(index);
            index = (int) unsignedIndex;
        }

        return _spells.IsInBounds(index) ? _spells?.ElementAt(index) : new GameFileDataSpell();
    }

    public static string GetLocalizationName(string uniqueName)
    {
        var spell = _spells.FirstOrDefault(x => x.UniqueName == uniqueName);
        if (spell is null)
        {
            return uniqueName;
        }

        return string.IsNullOrEmpty(spell.NameLocatag) ? uniqueName : LocalizationController.Translation(spell.NameLocatag);
    }

    public static string GetLocalizationDescription(string uniqueName)
    {
        var spell = _spells.FirstOrDefault(x => x.UniqueName == uniqueName);
        if (spell is null)
        {
            return uniqueName;
        }

        return string.IsNullOrEmpty(spell.DescriptionLocatag) ? uniqueName : LocalizationController.Translation(spell.DescriptionLocatag);
    }

    public static async Task<bool> LoadDataAsync()
    {
        var gameFilesDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.GameFilesDirectoryName);
        var regularDataFilePath = Path.Combine(gameFilesDirPath, Settings.Default.SpellDataFileName);

        if (!File.Exists(regularDataFilePath))
        {
            _spells = new List<GameFileDataSpell>();
        }

        var document = XDocument.Load(regularDataFilePath);
        _spells = BuildSpells([.. document.Root!.Elements()]);

        await Task.CompletedTask;

        return _spells.Count >= 0;
    }

    public static List<GameFileDataSpell> BuildSpells(List<XElement> elements)
    {
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
                var passiveSpell = CreateGameFileDataSpell(index++, element);
                if (passiveSpell != null)
                {
                    spells.Add(passiveSpell);
                }
            }
            else if (element.Name == "activespell")
            {
                var activeSpell = CreateGameFileDataSpell(index++, element);
                if (activeSpell != null)
                {
                    spells.Add(activeSpell);
                }

                if (element.Element("channelingspell") != null)
                {
                    var channelingSpell = CreateGameFileDataSpell(index++, element);
                    if (channelingSpell != null)
                    {
                        spells.Add(channelingSpell);
                    }
                }
            }
            else if (element.Name == "togglespell")
            {
                var toggleSpell = CreateGameFileDataSpell(index++, element);
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

    private static GameFileDataSpell CreateGameFileDataSpell(int index, XElement element)
    {
        var uniqueName = element.Attribute("uniquename")?.Value ?? string.Empty;
        var nameLocatag = element.Attribute("namelocatag")?.Value ?? string.Empty;
        var descriptionLocatag = element.Attribute("descriptionlocatag")?.Value ?? string.Empty;
        var target = element.Attribute("target")?.Value ?? string.Empty;
        var category = element.Attribute("category")?.Value ?? string.Empty;

        if (!string.IsNullOrEmpty(uniqueName))
        {
            return new GameFileDataSpell()
            {
                Index = index,
                UniqueName = uniqueName,
                Target = target,
                Category = category,
                NameLocatag = nameLocatag,
                DescriptionLocatag = descriptionLocatag
            };
        }

        return null;
    }
}