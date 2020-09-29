using log4net;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;

namespace StatisticsAnalysisTool.Common
{
    public class SoundController
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static List<FileInformation> AlertSounds { get; set; }

        public static void InitializeSoundFilesFromDirectory()
        {
            if (AlertSounds != null)
            {
                return;
            }
            
            var soundFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SoundDirectoryName);

            if (!Directory.Exists(soundFilePath))
            {
                return;
            }

            var files = DirectoryController.GetFiles(soundFilePath, "*.wav");

            if (files == null)
            {
                return;
            }

            if (AlertSounds == null)
            {
                AlertSounds = new List<FileInformation>();
            }

            foreach (var file in files)
            {
                var fileInformation = new FileInformation(Path.GetFileNameWithoutExtension(file), file);
                AlertSounds.Add(fileInformation);
            }
        }

        public static void PlayAlertSound()
        {
            try
            {
                var player = new SoundPlayer(GetCurrentSound());
                player.Load();
                player.Play();
                player.Dispose();
            }
            catch (Exception e) when (e is InvalidOperationException || e is UriFormatException || e is FileNotFoundException)
            {
                Log.Error(nameof(PlayAlertSound), e);
            }
        }

        private static string GetCurrentSound()
        {
            var currentSound = AlertSounds.FirstOrDefault(s => s.FileName == Settings.Default.SelectedAlertSound);
            return currentSound?.FilePath ?? string.Empty;
        }
    }
}