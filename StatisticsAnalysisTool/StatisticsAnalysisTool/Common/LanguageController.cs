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
        private static string _currentLanguage;
        public static CultureInfo DefaultCultureInfo = (_currentLanguage != null) ? new CultureInfo(_currentLanguage) : new CultureInfo("en-US");
        public static readonly List<FileInfo> FileInfos = new List<FileInfo>();

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
            _currentLanguage = fileInfo.FileName;
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
