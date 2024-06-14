using System.Text.Json;

namespace StatisticAnalysisTool.Extractor;

internal class SpellData
{
    public static async Task ExtractAsJsonAsync(LocalizationData localizationData, string outputFolderPath)
    {
        var spells = new List<SpellLoc>();

        foreach (var spellName in localizationData.SpellLocalizedNames)
        {
            var tuId = spellName.Key;
            var descriptions = localizationData.SpellLocalizedDescriptions.FirstOrDefault(x => x.Key.Contains(tuId)).Value ?? new Dictionary<string, string>();

            var uniqueName = tuId.Replace("@SPELLS_", "").Replace("_DESC", "");

            spells.Add(new SpellLoc
            {
                UniqueName = uniqueName,
                LocalizedNames = spellName.Value,
                LocalizedDescriptions = descriptions
            });
        }

        var json = JsonSerializer.Serialize(spells, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var outputFilePath = Path.Combine(outputFolderPath, "spells-localization.json");
        await File.WriteAllTextAsync(outputFilePath, json);
    }

    internal class SpellLoc
    {
        public string? UniqueName { get; init; }
        public Dictionary<string, string>? LocalizedNames { get; init; }
        public Dictionary<string, string>? LocalizedDescriptions { get; init; }
    }
}