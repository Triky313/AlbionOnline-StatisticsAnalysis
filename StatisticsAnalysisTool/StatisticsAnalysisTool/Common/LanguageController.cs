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
        private static Dictionary<string, string> _translations;
        private static string _currentCultureInfo;

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
            CurrentCultureInfo = new CultureInfo(fileInfo.FileName);
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
        
        private static void InitializeLanguageFiles()
        {
            var languageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName);

            if (Directory.Exists(languageFilePath))
            {
                var files = DirectoryController.GetFiles(languageFilePath, "*.xml");

                if (files == null)
                    return;

                if(FileInfos == null)
                    FileInfos = new List<FileInfo>();

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

            if (SetLanguage(Settings.Default.DefaultLanguageCultureIetfLanguageTag))
                return true;

            if (SetLanguage(FileInfos.FirstOrDefault()?.FileName))
                return true;

            return false;
        }

        public static List<FileInfo> FileInfos { get; set; }

        public static CultureInfo CurrentCultureInfo
        {
            get => (_currentCultureInfo != null) ? new CultureInfo(_currentCultureInfo) : new CultureInfo(Settings.Default.DefaultLanguageCultureIetfLanguageTag);
            set => _currentCultureInfo = value.IetfLanguageTag;
        }

        public class FileInfo
        {
            public FileInfo() { }

            public FileInfo(string fileName, string filePath)
            {
                FileName = fileName;
                FilePath = filePath;
            }

            public string FileName { get; set; }
            public string FilePath { get; set; }
            public string EnglishName => CultureInfo.CreateSpecificCulture(FileName).EnglishName;
            public string NativeName => CultureInfo.CreateSpecificCulture(FileName).NativeName;
        }

    }
}
