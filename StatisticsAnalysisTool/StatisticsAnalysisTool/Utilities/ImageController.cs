using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Utilities
{
    class ImageController
    {
        private static readonly string ImageDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            Settings.Default.ImageResources);

        public static BitmapImage GetItemImage(string webPath = null, int pixelHeight = 100, int pixelWidth = 100, bool freeze = false)
        {
            BitmapImage image;

            if (webPath == null)
                return null;

            var webUri = new Uri(webPath, UriKind.Absolute);
            var filename = Path.GetFileName(webUri.AbsolutePath);

            var localFilePath = Path.Combine(ImageDir, filename);
            
            if (File.Exists(localFilePath))
            {
                image = SetImage(localFilePath, pixelHeight, pixelWidth, freeze);
            }
            else
            {
                image = SetImage(webPath, pixelHeight, pixelWidth, freeze);
                SaveImageLocal(image, localFilePath);
            }

            return image;
        }

        public static void SaveImageLocal(BitmapImage image, string localFilePath)
        {
            if(!DirectoryController.CreateDirectoryWhenNotExists(ImageDir) && !Directory.Exists(ImageDir))
                return;

            image.DownloadCompleted += (sender, args) =>
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)sender));
                using (var filestream = new FileStream(localFilePath, FileMode.Create))
                {
                    encoder.Save(filestream);
                }
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
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
            }

            return null;
        }

        public static BitmapImage GetImageFromResource(string resourcePath, int pixelHeight, int pixelWidth, bool freeze)
        {
            var sri = Application.GetResourceStream(new Uri(resourcePath, UriKind.Absolute));
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnDemand;
            bmp.DecodePixelHeight = pixelHeight;
            bmp.DecodePixelWidth = pixelWidth;
            bmp.StreamSource = sri?.Stream;
            bmp.EndInit();
            if (freeze)
                bmp.Freeze();

            return bmp;
        }

        public static int LocalImagesCounter()
        {
            try
            {
                if (Directory.Exists(ImageDir))
                {
                    return Directory.GetFiles(ImageDir, "*", SearchOption.TopDirectoryOnly).Length;
                } else
                {
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

    }
}
