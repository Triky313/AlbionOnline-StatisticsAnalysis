using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace StatisticsAnalysisTool.Common
{
    public static class LanguageController
    {
        public static string CurrentLanguage;
        public static CultureInfo DefaultCultureInfo = (CurrentLanguage != null) ? new CultureInfo(CurrentLanguage) : new CultureInfo("en-US");
        public static readonly List<FileInfo> FileInfos = new List<FileInfo>();

        public static readonly Dictionary<LocalizedNamesEnum, string> LocalizedNamesDictionary = new Dictionary<LocalizedNamesEnum, string>
        {
            {LocalizedNamesEnum.EnUs, "EN-US" },
            {LocalizedNamesEnum.DeDe, "DE-DE" },
            {LocalizedNamesEnum.KoKr, "KO-KR" },
            {LocalizedNamesEnum.RuRu, "RU-RU" },
            {LocalizedNamesEnum.PlPl, "PL-PL" },
            {LocalizedNamesEnum.PtBr, "PT-BR" },
            {LocalizedNamesEnum.FrFr, "FR-FR" },
            {LocalizedNamesEnum.EsEs, "ES-ES" },
            {LocalizedNamesEnum.ZhCh, "ZH-CH" }
        };

        public enum LocalizedNamesEnum
        {
            EnUs, DeDe, KoKr, RuRu, PlPl, PtBr, FrFr, EsEs, ZhCh
        }

        private static Dictionary<string, string> _translations;


        public static string Translation(string key)
        {
            try
            {
                if (_translations.TryGetValue(key, out var value))
                    return (!string.IsNullOrEmpty(value)) ? value : key;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }
            return key;
        }

        public static bool SetLanguage(string lang)
        {
            var fileInfo = (from fi in FileInfos where fi.FileName == lang select new FileInfo(fi.FileName, fi.FilePath)).FirstOrDefault();

            if (fileInfo == null)
                return false;

            ReadAndAddLanguageFile(fileInfo.FilePath);
            CurrentLanguage = fileInfo.FileName;
            return true;
        }

        private static void ReadAndAddLanguageFile(string filePath)
        {
            _translations = null;
            _translations = new Dictionary<string, string>();
            var xmlReader = XmlReader.Create(filePath);
            while (xmlReader.Read())
            {
                if (xmlReader.Name == "translation" && xmlReader.HasAttributes)
                {
                    AddTranslationsToDictionary(xmlReader);
                }
            }
        }

        private static void AddTranslationsToDictionary(XmlReader xmlReader)
        {
            while (xmlReader.MoveToNextAttribute())
            {
                if (_translations.ContainsKey(xmlReader.Value))
                {
                    MessageBox.Show($"{Translation("DOUBLE_VALUE_EXISTS_IN_THE_LANGUAGE_FILE")}: {xmlReader.Value}");
                } 
                else if (xmlReader.Name == "name")
                {
                    _translations.Add(xmlReader.Value, xmlReader.ReadString());
                }
            }
        }
        
        public static void InitializeLanguageFiles()
        {
            var languageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName);

            if (Directory.Exists(languageFilePath))
            {
                var files = DirectoryController.GetFiles(languageFilePath, "*.xml");

                if (files == null)
                    return;

                foreach (var file in files)
                {
                    var fileNameWithoutExtension = new FileInfo(Path.GetFileNameWithoutExtension(file), file);
                    FileInfos.Add(fileNameWithoutExtension);
                }
            }
        }

        public static bool SetFirstLanguageIfPossible()
        {
            InitializeLanguageFiles();

            if (SetLanguage(CultureInfo.CurrentCulture.Name))
                return true;

            if (SetLanguage(Settings.Default.CurrentLanguageCulture))
                return true;

            if (SetLanguage(FileInfos.FirstOrDefault()?.FileName))
                return true;

            return false;
        }

        public class FileInfo
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string EnglishName => CultureInfo.CreateSpecificCulture(FileName).EnglishName;
            public string NativeName => CultureInfo.CreateSpecificCulture(FileName).NativeName;

            public FileInfo() { }

            public FileInfo(string fileName, string filePath)
            {
                FileName = fileName;
                FilePath = filePath;
            }
        }

    }
}
