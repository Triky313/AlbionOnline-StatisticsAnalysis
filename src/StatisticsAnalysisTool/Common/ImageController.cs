using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Serilog;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Common;

internal static class ImageController
{
    private static readonly string ItemImagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.ImageResources);
    private static readonly string SpellImagesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SpellImageResources);

    private static Dictionary<string, BitmapImage> downloading = new();

    #region Item image

    public static BitmapImage GetItemImage(string uniqueName = null, int pixelHeight = 100, int pixelWidth = 100, bool freeze = false)
    {
        string defaultImagePath = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Resources/Trash.png";
        try
        {
            if (string.IsNullOrEmpty(uniqueName) || downloading.ContainsKey(uniqueName))
            {
                return CreateBitmapImage(defaultImagePath);
            }

            var localFilePath = Path.Combine(ItemImagesDirectory, uniqueName);

            if (File.Exists(localFilePath))
            {
                return GetImageFromLocal(localFilePath, pixelHeight, pixelWidth, freeze);
            }
            else
            {

                var image = SetImage($"https://render.albiononline.com/v1/item/{uniqueName}", uniqueName, pixelHeight, pixelWidth, freeze);
                SaveImageLocal(image, uniqueName, localFilePath, ItemImagesDirectory);
                return image;
            }
        }
        catch
        {
            return CreateBitmapImage(defaultImagePath);
        }
    }

    private static BitmapImage CreateBitmapImage(string path)
    {
        return new BitmapImage(new Uri(path, UriKind.Absolute));
    }

    public static async Task<int> LocalImagesCounterAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                return Directory.Exists(ItemImagesDirectory) ? Directory.GetFiles(ItemImagesDirectory, "*", SearchOption.TopDirectoryOnly).Length : 0;
            }
            catch
            {
                return 0;
            }
        });
    }

    #endregion

    #region Spell image

    public static BitmapImage GetSpellImage(string uniqueName = null, int pixelHeight = 100, int pixelWidth = 100, bool freeze = false)
    {
        try
        {
            if (string.IsNullOrEmpty(uniqueName))
            {
                return null;
            }

            BitmapImage image;
            var localFilePath = Path.Combine(SpellImagesDirectory, uniqueName);

            if (File.Exists(localFilePath))
            {
                image = GetImageFromLocal(localFilePath, pixelHeight, pixelWidth, freeze);
            }
            else
            {
                image = SetImage($"https://render.albiononline.com/v1/spell/{uniqueName}", uniqueName, pixelHeight, pixelWidth, freeze);
                SaveImageLocal(image, uniqueName, localFilePath, SpellImagesDirectory);
            }

            return image;
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Utilities
    private static BitmapImage GetImageFromLocal(string localResourcePath, int pixelHeight, int pixelWidth, bool freeze)
    {
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnDemand;
            bmp.DecodePixelHeight = pixelHeight;
            bmp.DecodePixelWidth = pixelWidth;
            bmp.UriSource = new Uri(localResourcePath);
            bmp.EndInit();

            if (freeze)
            {
                bmp.Freeze();
            }

            return bmp;
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return null;
        }
    }

    private static void SaveImageLocal(BitmapSource image, string uniqueName, string localFilePath, string localDirectory)
    {
        if (!DirectoryController.CreateDirectoryWhenNotExists(localDirectory) && !Directory.Exists(localDirectory))
        {
            return;
        }

        image.DownloadCompleted += (sender, _) =>
        {

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(((BitmapImage) sender)!));
            using var fileStream = new FileStream(localFilePath, FileMode.Create);
            encoder.Save(fileStream);
            downloading.Remove(uniqueName);
        };
    }

    private static BitmapImage SetImage(string webPath, string uniqueName, int pixelHeight, int pixelWidth, bool freeze)
    {
        if (webPath == null)
        {
            return null;
        }

        try
        {
            var userImage = new BitmapImage
            {
                CacheOption = BitmapCacheOption.OnDemand
            };

            downloading.TryAdd(uniqueName, userImage);
            userImage.BeginInit();
            userImage.DecodePixelHeight = pixelHeight;
            userImage.DecodePixelWidth = pixelWidth;
            userImage.UriSource = new Uri(webPath);
            userImage.EndInit();

            if (freeze)
            {
                userImage.Freeze();
            }

            return userImage;
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error($"{MethodBase.GetCurrentMethod()?.DeclaringType}: {e.Message}");
            return null;
        }
    }

    #endregion
}