using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Utilities
{
    public static class LanguageController
    {
        public static string CurrentLanguage;
        public static CultureInfo DefaultCultureInfo = (CurrentLanguage != null) ? new CultureInfo(CurrentLanguage) : new CultureInfo("en-US");
        public static readonly List<FileInfo> FileInfos = new List<FileInfo>();
        
        private static Dictionary<string, string> _translations;

        /// <summary>
        /// Replaces placeholder in a text of the selected language.
        /// </summary>
        /// <param name="key">Placeholder</param>
        /// <returns></returns>
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

            ReadLanguageFile(fileInfo.FilePath);
            CurrentLanguage = fileInfo.FileName;
            return true;
        }

        private static void ReadLanguageFile(string filePath)
        {
            _translations = null;
            _translations = new Dictionary<string, string>();
            var xmlReader = XmlReader.Create(filePath);
            while (xmlReader.Read())
            {
                if (xmlReader.Name == "translation")
                {
                    if (xmlReader.HasAttributes)
                    {
                        while (xmlReader.MoveToNextAttribute())
                        {
                            if (_translations.ContainsKey(xmlReader.Value))
                            {
                                MessageBox.Show($"{Translation("DOUBLE_VALUE_EXISTS_IN_THE_LANGUAGE_FILE")}: {xmlReader.Value}");
                                continue;
                            }

                            if (xmlReader.Name == "name")
                            {
                                _translations.Add(xmlReader.Value, xmlReader.ReadString());
                            }
                        }
                    }
                }
            }
        }

        public static void InitializeLanguageFiles()
        {
            if (Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName)))
            {
                string[] files = Directory.GetFiles(Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName), "*.xml");
                foreach (var file in files)
                {
                    var fi = new FileInfo(Path.GetFileNameWithoutExtension(file), file);
                    FileInfos.Add(fi);
                }
            }
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
