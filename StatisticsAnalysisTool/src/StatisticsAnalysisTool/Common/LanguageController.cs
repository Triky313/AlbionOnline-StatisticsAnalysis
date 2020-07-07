using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
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
        private static CultureInfo _currentCultureInfo;

        public static CultureInfo CurrentCultureInfo {
            get => _currentCultureInfo ?? new CultureInfo(Settings.Default.DefaultLanguageCultureName);
            set
            {
                _currentCultureInfo = value;
                Settings.Default.CurrentLanguageCultureName = value.TextInfo.CultureName;
            }
        }

        public static bool InitializeLanguage()
        {
            try
            {
                CurrentCultureInfo = new CultureInfo(Settings.Default.CurrentLanguageCultureName ?? Settings.Default.DefaultLanguageCultureName);
                if (SetLanguage())
                    return true;

                throw new CultureNotFoundException();
            }
            catch (CultureNotFoundException)
            {
                CurrentCultureInfo = new CultureInfo(Settings.Default.DefaultLanguageCultureName);
                if (SetLanguage())
                    return true;

                if(string.IsNullOrEmpty(CurrentCultureInfo.TextInfo.CultureName))
                    MessageBox.Show("No culture info found!", Translation("ERROR"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        public static string Translation(string key)
        {
            try
            {
                if (_translations.TryGetValue(key, out var value))
                    return (!string.IsNullOrEmpty(value)) ? value : key;
            }
            catch (ArgumentNullException)
            {
                return "TRANSLATION-ERROR";
            }
            return key;
        }

        public static bool SetLanguage()
        {
            InitializeLanguageFilesFromDirectory();

            try
            {
                if (LanguageFiles == null)
                    throw new FileNotFoundException();

                var fileInfo = (from file in LanguageFiles
                    where file.FileName.ToUpper() == CurrentCultureInfo.TextInfo.CultureName.ToUpper()
                    select new FileInfo(file.FileName, file.FilePath)).FirstOrDefault();

                if (fileInfo == null)
                    return false;

                if (!ReadAndAddLanguageFile(fileInfo.FilePath))
                    return false;

                CurrentCultureInfo = new CultureInfo(fileInfo.FileName);
                return true;
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message, Translation("ERROR"));
                return false;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Language file not found. ", Translation("ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private static bool ReadAndAddLanguageFile(string filePath)
        {
            _translations = new Dictionary<string, string>();
            try
            {
                var xmlReader = XmlReader.Create(filePath);
                while (xmlReader.Read())
                {
                    if (xmlReader.Name == "translation" && xmlReader.HasAttributes)
                    {
                        AddTranslationsToDictionary(xmlReader);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Translation("ERROR"));
                return false;
            }
            return true;
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
        
        private static void InitializeLanguageFilesFromDirectory()
        {
            if (LanguageFiles != null)
                return;

            var languageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName);

            if (!Directory.Exists(languageFilePath)) 
                return;

            var files = DirectoryController.GetFiles(languageFilePath, "*.xml");

            if (files == null)
                return;

            if(LanguageFiles == null)
                LanguageFiles = new List<FileInfo>();

            foreach (var file in files)
            {
                var fileNameWithoutExtension = new FileInfo(Path.GetFileNameWithoutExtension(file), file);
                LanguageFiles.Add(fileNameWithoutExtension);
            }
        }
        
        public static List<FileInfo> LanguageFiles { get; set; }

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
