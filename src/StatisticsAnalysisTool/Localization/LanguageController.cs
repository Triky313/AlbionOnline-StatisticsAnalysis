using Serilog;
using StatisticsAnalysisTool.Common;
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
using System.Windows;
using System.Xml;
using System.Xml.Linq;

namespace StatisticsAnalysisTool.Localization;

public static class LanguageController
{
    private static readonly Dictionary<string, string> Translations = new();

    public static List<FileInformation> LanguageFiles { get; set; } = new();
    public static bool IsLanguageSelected { get; private set; }

    public static bool Init()
    {
        // Load language file information
        LanguageFiles = InitLanguageFiles();

        // Set culture information
        var cultureInfo = Culture.GetCulture(SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag);
        IsLanguageSelected = IsCultureInfoAvailableAsLanguageFile(cultureInfo);

        // Set language
        if (IsLanguageSelected && SetLanguage(cultureInfo))
        {
            return true;
        }

        if (SetLanguageWithDialogWindow())
        {
            return true;
        }

        Log.Error("{message}: Language file not found, tool closed.", MethodBase.GetCurrentMethod()?.DeclaringType);
        return false;
    }

    public static List<FileInformation> InitLanguageFiles()
    {
        var languageFilesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName);
        var languageFiles = FileController.GetFileInformation(languageFilesDir);
        return SetLanguageFilesInformation(languageFiles);
    }

    private static List<FileInformation> SetLanguageFilesInformation(List<FileInformation> files)
    {
        var fullInfoLanguages = new List<FileInformation>();

        SetPercentageTranslationValues(files);

        foreach (var file in files)
        {
            var cultureInfo = CultureInfo.CreateSpecificCulture(file.FileName);
            fullInfoLanguages.Add(new FileInformation(file.FileName, string.Empty)
            {
                EnglishName = cultureInfo.EnglishName,
                NativeName = cultureInfo.NativeName,
                PercentageTranslations = file.PercentageTranslations,
                FilePath = file.FilePath
            });
        }

        return fullInfoLanguages;
    }

    public static bool SetLanguageWithDialogWindow()
    {
        var dialogWindow = new LanguageSelectionWindow();
        var dialogResult = dialogWindow.ShowDialog();

        if (dialogResult is not true)
        {
            return false;
        }

        var languageSelectionWindowViewModel = (LanguageSelectionWindowViewModel) dialogWindow.DataContext;
        var selectedLanguage = languageSelectionWindowViewModel.SelectedFileInformation;

        var culture = new CultureInfo(selectedLanguage.FileName);
        SetLanguage(culture);
        return true;
    }

    public static bool SetLanguage(CultureInfo cultureInfo)
    {
        Culture.SetCulture(cultureInfo);
        var languageFileInfo = GetLanguageFileInfo(cultureInfo);
        return LoadTranslations(languageFileInfo.FilePath);
    }

    private static bool IsCultureInfoAvailableAsLanguageFile(CultureInfo cultureInfo)
    {
        if (cultureInfo == null)
        {
            return false;
        }

        var fileInfos = LanguageFiles.FirstOrDefault(x => string.Equals(x.FileName, cultureInfo.TextInfo.CultureName, StringComparison.CurrentCultureIgnoreCase));

        return fileInfos != null;
    }

    public static FileInformation GetLanguageFileInfo(CultureInfo cultureInfo)
    {
        if (LanguageFiles.Count <= 0)
        {
            throw new FileNotFoundException();
        }

        var fileInfos = LanguageFiles.FirstOrDefault(x => string.Equals(x.FileName, cultureInfo.TextInfo.CultureName, StringComparison.CurrentCultureIgnoreCase));
        if (fileInfos == null)
        {
            throw new FileNotFoundException();
        }

        return fileInfos;
    }

    private static bool LoadTranslations(string filePath)
    {
        Translations.Clear();
        var xmlReader = XmlReader.Create(filePath, new XmlReaderSettings()
        {
            Async = true
        });

        while (xmlReader.Read())
        {
            if (xmlReader.Name != "translation" || !xmlReader.HasAttributes)
            {
                continue;
            }

            var translationLine = GetTranslationLine(xmlReader);

            if (string.IsNullOrEmpty(translationLine.Item1) || string.IsNullOrEmpty(translationLine.Item2))
            {
                continue;
            }

            Translations.Add(translationLine.Item1, translationLine.Item2);
        }

        return true;
    }

    public static string Translation(string key, List<string> placeholders, List<string> replacements)
    {
        try
        {
            if (Translations.TryGetValue(key, out var value))
            {
                if (string.IsNullOrEmpty(value) || placeholders.Count != replacements.Count)
                {
                    return key;
                }

                for (int i = 0; i < placeholders.Count; i++)
                {
                    value = value.Replace("{" + placeholders[i] + "}", replacements[i]);
                }

                return value;
            }
        }
        catch (ArgumentNullException)
        {
            return "TRANSLATION-ERROR";
        }

        return key;
    }

    public static string Translation(string key)
    {
        try
        {
            if (Translations.TryGetValue(key, out var value))
            {
                return !string.IsNullOrEmpty(value) ? value : key;
            }
        }
        catch (ArgumentNullException)
        {
            return "TRANSLATION-ERROR";
        }

        return key;
    }

    private static Tuple<string, string> GetTranslationLine(XmlReader xmlReader)
    {
        var name = xmlReader.GetAttribute("name");
        var value = xmlReader.ReadString();

        if (name == null || string.IsNullOrEmpty(value))
        {
            return new Tuple<string, string>(string.Empty, string.Empty);
        }

        if (Translations.ContainsKey(name))
        {
            Log.Warning("{message}", $"{MethodBase.GetCurrentMethod()?.DeclaringType}: {Translation("DOUBLE_VALUE_EXISTS_IN_THE_LANGUAGE_FILE")}: {name}");
            return new Tuple<string, string>(string.Empty, string.Empty);
        }

        return new Tuple<string, string>(name, value);
    }

    private static void SetPercentageTranslationValues(List<FileInformation> files, string mainLanguageFileName = "en-US")
    {
        var mainLanguageFile = files.FirstOrDefault(x => string.Equals(x.FileName, mainLanguageFileName, StringComparison.CurrentCultureIgnoreCase));
        if (mainLanguageFile == null)
        {
            return;
        }

        var mainLanguageFileCount = CountTranslations(mainLanguageFile.FilePath);
        mainLanguageFile.PercentageTranslations = 100;

        foreach (var fileInformation in files.Where(x => x.FileName != mainLanguageFileName).ToList())
        {
            var countTranslations = CountTranslations(fileInformation.FilePath);
            double percentageValue = 100d / mainLanguageFileCount * countTranslations;

            var fileInfo = files.FirstOrDefault(x => x.FileName == fileInformation.FileName);
            if (fileInfo != null)
            {
                fileInfo.PercentageTranslations = percentageValue > 100d ? 100 : percentageValue;
            }
        }
    }

    private static int CountTranslations(string filePath)
    {
        var xml = XDocument.Load(filePath);
        return xml.Descendants("translation").Count();
    }
}