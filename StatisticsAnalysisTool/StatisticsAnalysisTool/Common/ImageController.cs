using log4net;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Common
{
    internal class ImageController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string ImageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.ImageResources);

        public static BitmapImage GetItemImage(string uniqueName = null, int pixelHeight = 100, int pixelWidth = 100, bool freeze = false)
        {
            try
            {
                if (string.IsNullOrEmpty(uniqueName))
                {
                    return new BitmapImage(new Uri(
                        @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Resources/Trash.png",
                        UriKind.Absolute));
                }

                BitmapImage image;
                var localFilePath = Path.Combine(ImageDir, uniqueName);

                if (File.Exists(localFilePath))
                {
                    image = GetImageFromResource(localFilePath, pixelHeight, pixelWidth, freeze);
                }
                else
                {
                    image = SetImage($"https://render.albiononline.com/v1/item/{uniqueName}", pixelHeight, pixelWidth, freeze);
                    SaveImageLocal(image, localFilePath);
                }

                return image;
            }
            catch
            {
                return new BitmapImage(new Uri(
                    @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Resources/Trash.png",
                    UriKind.Absolute));
            }
        }

        public static void SaveImageLocal(BitmapImage image, string localFilePath)
        {
            if (!DirectoryController.CreateDirectoryWhenNotExists(ImageDir) && !Directory.Exists(ImageDir))
                return;

            image.DownloadCompleted += (sender, args) =>
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage) sender));
                using var fileStream = new FileStream(localFilePath, FileMode.Create);
                encoder.Save(fileStream);
            };
        }

        public static BitmapImage SetImage(string webPath, int pixelHeight, int pixelWidth, bool freeze)
        {
            if (webPath == null)
                return null;

            try
            {
                var request = WebRequest.Create(new Uri(webPath));
                var userImage = new BitmapImage();
                userImage.BeginInit();
                userImage.CacheOption = BitmapCacheOption.OnDemand;
                userImage.DecodePixelHeight = pixelHeight;
                userImage.DecodePixelWidth = pixelWidth;
                userImage.UriSource = request.RequestUri;
                userImage.EndInit();
                if (freeze)
                    userImage.Freeze();

                return userImage;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error($"{MethodBase.GetCurrentMethod().DeclaringType} - SetImage: {e.Message}");
                return null;
            }
        }

        public static BitmapImage GetImageFromResource(string resourcePath, int pixelHeight, int pixelWidth, bool freeze)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnDemand;
                bmp.DecodePixelHeight = pixelHeight;
                bmp.DecodePixelWidth = pixelWidth;
                bmp.UriSource = new Uri(resourcePath);
                bmp.EndInit();
                if (freeze)
                    bmp.Freeze();

                return bmp;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error($"{MethodBase.GetCurrentMethod().DeclaringType} - SetImage: {e.Message}");
                return null;
            }
        }

        public static async Task<int> LocalImagesCounterAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    return Directory.Exists(ImageDir) ? Directory.GetFiles(ImageDir, "*", SearchOption.TopDirectoryOnly).Length : 0;
                }
                catch
                {
                    return 0;
                }
            });
        }
    }
}