using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace StatisticsAnalysisTool.Localization;

public class LocalizationController
{
    private static readonly Dictionary<string, Dictionary<string, string>> Translations = new();

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
        var culture = SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag;

        try
        {
            if (Translations.TryGetValue(culture, out var cultureTranslations) && cultureTranslations.TryGetValue(key, out var translation))
            {
                if (placeholders != null && replacements != null)
                {
                    if (placeholders.Count != replacements.Count)
                    {
                        return key;
                    }

                    for (int i = 0; i < placeholders.Count; i++)
                    {
                        translation = translation.Replace("{" + placeholders[i] + "}", replacements[i]);
                    }
                }

                return !string.IsNullOrEmpty(translation) ? translation : key;
            }
        }
        catch (ArgumentNullException)
        {
            return "TRANSLATION-ERROR";
        }

        return key;
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
}