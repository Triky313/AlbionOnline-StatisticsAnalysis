using log4net;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml;

namespace StatisticsAnalysisTool.Common
{
    public static class LanguageController
    {
        public static List<FileInformation> LanguageFiles { get; set; }

        private static readonly Dictionary<string, string> _translations = new Dictionary<string, string>();
        private static CultureInfo _currentCultureInfo;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static CultureInfo CurrentCultureInfo {
            get => _currentCultureInfo;
            set {
                _currentCultureInfo = value;
                Settings.Default.CurrentLanguageCultureName = value.TextInfo.CultureName;
            }
        }

        public static bool InitializeLanguage()
        {
            try
            {
                if (CurrentCultureInfo == null)
                {
                    if (!string.IsNullOrEmpty(Settings.Default.CurrentLanguageCultureName))
                    {
                        CurrentCultureInfo = new CultureInfo(Settings.Default.CurrentLanguageCultureName);
                    } 
                    else if (!string.IsNullOrEmpty(Settings.Default.DefaultLanguageCultureName))
                    {
                        CurrentCultureInfo = new CultureInfo(Settings.Default.DefaultLanguageCultureName);
                    }
                    else
                    {
                        throw new CultureNotFoundException();
                    }
                }

                if (SetLanguage())
                {
                    return true;
                }

                CurrentCultureInfo = new CultureInfo(Settings.Default.DefaultLanguageCultureName);
                if (SetLanguage())
                {
                    return true;
                }

                throw new CultureNotFoundException();
            }
            catch (CultureNotFoundException)
            {
                MessageBox.Show("No culture info found!", Translation("ERROR"), MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        public static string Translation(string key)
        {
            try
            {
                if (_translations.TryGetValue(key, out var value))
                {
                    return (!string.IsNullOrEmpty(value)) ? value : key;
                }
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
                {
                    throw new FileNotFoundException();
                }

                var fileInfos = (from file in LanguageFiles
                                where file.FileName.ToUpper() == CurrentCultureInfo?.TextInfo.CultureName.ToUpper()
                                select new FileInformation(file.FileName, file.FilePath)).FirstOrDefault();

                if (fileInfos == null)
                {
                    return false;
                }

                if (!ReadAndAddLanguageFile(fileInfos.FilePath))
                {
                    return false;
                }

                return true;
            }
            catch (ArgumentNullException ex)
            {
                MessageBox.Show(ex.Message, Translation("ERROR"));
                Log.Error(nameof(SetLanguage), ex);
                return false;
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show("Language file not found. ", Translation("ERROR"), MessageBoxButton.OK, MessageBoxImage.Error);
                Log.Error(nameof(SetLanguage), ex);
                return false;
            }
        }

        private static bool ReadAndAddLanguageFile(string filePath)
        {
            try
            {
                _translations.Clear();
                var xmlReader = XmlReader.Create(filePath);
                while (xmlReader.Read())
                {
                    if (xmlReader.Name == "translation" && xmlReader.HasAttributes)
                    {
                        AddTranslationsToDictionary(xmlReader);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, Translation("ERROR"));
                Log.Error(nameof(ReadAndAddLanguageFile), e);
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
                    Log.Warn($"{nameof(AddTranslationsToDictionary)}: {Translation("DOUBLE_VALUE_EXISTS_IN_THE_LANGUAGE_FILE")}: {xmlReader.Value}");
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
            {
                return;
            }

            var languageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.LanguageDirectoryName);
            if (!Directory.Exists(languageFilePath))
            {
                return;
            }

            var files = DirectoryController.GetFiles(languageFilePath, "*.xml");
            if (files == null)
            {
                return;
            }

            if (LanguageFiles == null)
            {
                LanguageFiles = new List<FileInformation>();
            }

            foreach (var file in files)
            {
                var fileInfo = new FileInformation(Path.GetFileNameWithoutExtension(file), file);
                LanguageFiles.Add(fileInfo);
            }
        }
    }
}