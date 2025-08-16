using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Localization;

public class LocalizationController
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new(StringComparer.OrdinalIgnoreCase);
    private static ImmutableDictionary<string, ImmutableDictionary<string, string>> _gameLocalizations = ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty;

    public static bool Init()
    {
        var jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LocalizationDirectoryName, Settings.Default.LocalizationFileName);

        if (!File.Exists(jsonFilePath))
        {
            Log.Error("{message}: Localization file not found.", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }

        if (!LoadTranslations(jsonFilePath))
        {
            Log.Error("{message}: Localization file corrupted.", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }

        if (SetLanguageWithDialogWindow())
        {
            return true;
        }

        Log.Error("{message}: Language file not found, tool closed.", MethodBase.GetCurrentMethod()?.DeclaringType);
        return false;
    }

    private static bool LoadTranslations(string filePath)
    {
        Translations.Clear();

        var jsonString = File.ReadAllText(filePath);
        var localizationData = JsonSerializer.Deserialize<Localization>(jsonString);

        if (localizationData == null)
        {
            return false;
        }

        foreach (var translation in localizationData.Translations)
        {
            foreach (var translationValue in translation.Tuv)
            {
                if (!Translations.ContainsKey(translationValue.Lang))
                {
                    Translations[translationValue.Lang] = new Dictionary<string, string>();
                }
                Translations[translationValue.Lang][translation.TuId] = translationValue.Seg;
            }
        }

        return true;
    }

    public static bool SetLanguageWithDialogWindow()
    {
        if (!string.IsNullOrEmpty(SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag))
        {
            return true;
        }

        var dialogWindow = new LanguageSelectionWindow();
        var dialogResult = dialogWindow.ShowDialog();

        if (dialogResult is not true)
        {
            return false;
        }

        var languageSelectionWindowViewModel = (LanguageSelectionWindowViewModel) dialogWindow.DataContext;
        var selectedLanguage = languageSelectionWindowViewModel.SelectedFileInformation;

        Culture.SetCulture(Culture.GetCultureByIetfLanguageTag(selectedLanguage.FileName));
        return true;
    }

    public static string Translation(string key)
    {
        return Translation(key, null, null);
    }

    public static string Translation(string key, List<string> placeholders, List<string> replacements)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key;
        }

        var culture = SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag ?? string.Empty;

        if (Translations.TryGetValue(culture, out var transForCulture) && transForCulture.TryGetValue(key, out var t1) && !string.IsNullOrEmpty(t1))
        {
            return ApplyPlaceholders(t1, placeholders, replacements);
        }

        var gameLoc = Volatile.Read(ref _gameLocalizations);
        if (gameLoc != null && gameLoc.TryGetValue(key, out var langs) && langs.TryGetValue(culture, out var t2) && !string.IsNullOrEmpty(t2))
        {
            return ApplyPlaceholders(t2, placeholders, replacements);
        }

        return key;
    }

    private static string ApplyPlaceholders(string input, List<string> placeholders, List<string> replacements)
    {
        if (string.IsNullOrEmpty(input) || placeholders is null || replacements is null)
        {
            return input;
        }

        var count = Math.Min(placeholders.Count, replacements.Count);
        for (int i = 0; i < count; i++)
        {
            input = input.Replace("{" + placeholders[i] + "}", replacements[i]);
        }

        return input;
    }

    public static List<FileInformation> GetLanguageInformation()
    {
        var result = new List<FileInformation>();
        foreach (var lang in GetTranslationCoverage())
        {
            var culture = CultureInfo.GetCultureInfo(lang.Key);
            result.Add(new FileInformation()
            {
                FileName = culture.IetfLanguageTag,
                EnglishName = culture.EnglishName,
                NativeName = culture.NativeName,
                PercentageTranslations = lang.Value
            });
        }

        return result;
    }

    private static List<string> GetAvailableLanguages()
    {
        return Translations.Keys.ToList();
    }

    private static Dictionary<string, double> GetTranslationCoverage()
    {
        var availableLanguages = GetAvailableLanguages();
        var languageTranslationCounts = new Dictionary<string, int>();

        foreach (var language in availableLanguages)
        {
            languageTranslationCounts[language] = Translations[language].Count;
        }

        int maxTranslations = languageTranslationCounts.Values.Max();

        var languageCoverage = new Dictionary<string, double>();

        foreach (var language in availableLanguages)
        {
            double percentage = (double) languageTranslationCounts[language] / maxTranslations * 100;
            languageCoverage[language] = percentage;
        }

        return languageCoverage;
    }

    #region Load game data localization

    public static Task SetGameLocalizationsAsync(IReadOnlyDictionary<string, Dictionary<string, string>> allTu)
    {
        if (allTu is null || allTu.Count == 0)
        {
            Interlocked.Exchange(ref _gameLocalizations, ImmutableDictionary<string, ImmutableDictionary<string, string>>.Empty);
            return Task.CompletedTask;
        }

        var outer = ImmutableDictionary.CreateBuilder<string, ImmutableDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, langs) in allTu)
        {
            var inner = langs.ToImmutableDictionary(kv => kv.Key, kv => kv.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            outer[key] = inner;
        }

        Interlocked.Exchange(ref _gameLocalizations, outer.ToImmutable());
        return Task.CompletedTask;
    }

    #endregion
}