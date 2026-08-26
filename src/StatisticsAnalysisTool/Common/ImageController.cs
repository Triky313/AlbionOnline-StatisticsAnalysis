using Serilog;
using StatisticsAnalysisTool.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.Common;

internal static class ImageController
{
    private static readonly string ItemImagesDirectory = AppDataPaths.ImageResourcesDirectory;
    private static readonly string SpellImagesDirectory = AppDataPaths.SpellImageResourcesDirectory;
    private static readonly string DefaultItemImagePath = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Resources/Trash.png";
    private static readonly string GoldImagePath = @"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + ";component/" + "Resources/gold.png";
    private static readonly Lock CacheLock = new();
    private static readonly Dictionary<string, WeakReference<BitmapImage>> CachedImages = new(StringComparer.Ordinal);
    private static readonly HashSet<string> Downloading = new(StringComparer.Ordinal);

    private const int CacheCleanupThreshold = 512;

    internal static event EventHandler<ItemImageStoredEventArgs> ItemImageStored;

    #region Item image

    public static BitmapImage GetItemImage(string uniqueName = null, int pixelHeight = 100, int pixelWidth = 100, bool freeze = false)
    {
        return GetItemImage(uniqueName, null, pixelHeight, pixelWidth, freeze);
    }

    public static BitmapImage GetItemImageWithQuality(string uniqueName, int qualityLevel, int pixelHeight = 100, int pixelWidth = 100, bool freeze = false)
    {
        int? validQualityLevel = qualityLevel is >= 1 and <= 5 ? qualityLevel : null;
        return GetItemImage(uniqueName, validQualityLevel, pixelHeight, pixelWidth, freeze);
    }

    private static BitmapImage GetItemImage(string uniqueName, int? qualityLevel, int pixelHeight, int pixelWidth, bool freeze)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(uniqueName))
            {
                return CreateBitmapImage(DefaultItemImagePath);
            }

            var imageName = qualityLevel.HasValue ? $"{uniqueName}_quality_{qualityLevel.Value}" : uniqueName;
            var webPath = qualityLevel.HasValue
                ? $"https://render.albiononline.com/v1/item/{uniqueName}?quality={qualityLevel.Value}"
                : $"https://render.albiononline.com/v1/item/{uniqueName}";
            Action onImageSaved = qualityLevel.HasValue
                ? () => ItemImageStored?.Invoke(null, new ItemImageStoredEventArgs(uniqueName, qualityLevel.Value))
                : null;

            return GetImage("item", imageName, pixelHeight, pixelWidth, freeze, ItemImagesDirectory, webPath, onImageSaved)
                   ?? CreateBitmapImage(DefaultItemImagePath);
        }
        catch
        {
            return CreateBitmapImage(DefaultItemImagePath);
        }
    }

    public static BitmapImage GetGoldImage()
    {
        return CreateBitmapImage(GoldImagePath);
    }

    private static BitmapImage CreateBitmapImage(string path)
    {
        var cacheKey = $"default|{path}";
        if (TryGetCachedImage(cacheKey, out var cachedImage))
        {
            return cachedImage;
        }

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
        bitmapImage.UriSource = new Uri(path, UriKind.Absolute);
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        CacheImage(cacheKey, bitmapImage);
        return bitmapImage;
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
            if (string.IsNullOrWhiteSpace(uniqueName))
            {
                return null;
            }

            return GetImage("spell", uniqueName, pixelHeight, pixelWidth, freeze, SpellImagesDirectory, $"https://render.albiononline.com/v1/spell/{uniqueName}");
        }
        catch
        {
            return null;
        }
    }

    #endregion

    #region Utilities

    private static BitmapImage GetImage(string imageType, string uniqueName, int pixelHeight, int pixelWidth, bool freeze, string localDirectory, string webPath, Action onImageSaved = null)
    {
        var cacheKey = $"{imageType}|{uniqueName}|{pixelHeight}|{pixelWidth}";
        if (TryGetCachedImage(cacheKey, out var cachedImage))
        {
            return cachedImage;
        }

        var localFilePath = Path.Combine(localDirectory, uniqueName);
        if (File.Exists(localFilePath))
        {
            var localImage = GetImageFromLocal(localFilePath, pixelHeight, pixelWidth, freeze);
            CacheImage(cacheKey, localImage);
            return localImage;
        }

        var downloadKey = $"{imageType}|{uniqueName}";
        if (!TryMarkAsDownloading(downloadKey))
        {
            return null;
        }

        var webImage = SetImage(webPath, downloadKey, pixelHeight, pixelWidth, freeze);
        if (webImage == null)
        {
            ClearDownloading(downloadKey);
            return null;
        }

        CacheImage(cacheKey, webImage);
        SaveImageLocal(webImage, downloadKey, localFilePath, localDirectory, onImageSaved);
        return webImage;
    }

    private static BitmapImage GetImageFromLocal(string localResourcePath, int pixelHeight, int pixelWidth, bool freeze)
    {
        try
        {
            var bmp = new BitmapImage();
            using var fileStream = new FileStream(localResourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelHeight = pixelHeight;
            bmp.DecodePixelWidth = pixelWidth;
            bmp.StreamSource = fileStream;
            bmp.EndInit();

            if (freeze || !bmp.IsFrozen)
            {
                bmp.Freeze();
            }

            return bmp;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return null;
        }
    }

    private static void SaveImageLocal(BitmapSource image, string downloadKey, string localFilePath, string localDirectory, Action onImageSaved)
    {
        if (!DirectoryController.CreateDirectoryWhenNotExists(localDirectory) && !Directory.Exists(localDirectory))
        {
            ClearDownloading(downloadKey);
            return;
        }

        if (!image.IsDownloading)
        {
            PersistImageToLocal(image, localFilePath, downloadKey, onImageSaved);
            return;
        }

        void OnDownloadCompleted(object sender, EventArgs args)
        {
            image.DownloadCompleted -= OnDownloadCompleted;
            image.DownloadFailed -= OnDownloadFailed;

            PersistImageToLocal(image, localFilePath, downloadKey, onImageSaved);
        }

        void OnDownloadFailed(object sender, ExceptionEventArgs args)
        {
            image.DownloadCompleted -= OnDownloadCompleted;
            image.DownloadFailed -= OnDownloadFailed;

            ClearDownloading(downloadKey);
        }

        image.DownloadCompleted += OnDownloadCompleted;
        image.DownloadFailed += OnDownloadFailed;
    }

    private static void PersistImageToLocal(BitmapSource image, string localFilePath, string downloadKey, Action onImageSaved)
    {
        var isSaved = false;
        try
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                encoder.Save(fileStream);
            }

            isSaved = true;
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        finally
        {
            ClearDownloading(downloadKey);
        }

        if (!isSaved)
        {
            return;
        }

        try
        {
            onImageSaved?.Invoke();
        }
        catch (Exception e)
        {
            Log.Error(e, "Item image saved notification failed");
        }
    }

    private static BitmapImage SetImage(string webPath, string downloadKey, int pixelHeight, int pixelWidth, bool freeze)
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

            userImage.BeginInit();
            userImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            userImage.DecodePixelHeight = pixelHeight;
            userImage.DecodePixelWidth = pixelWidth;
            userImage.UriSource = new Uri(webPath);
            userImage.EndInit();

            if (freeze && userImage.CanFreeze)
            {
                userImage.Freeze();
            }

            return userImage;
        }
        catch (Exception e)
        {
            ClearDownloading(downloadKey);
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error($"{MethodBase.GetCurrentMethod()?.DeclaringType}: {e.Message}");
            return null;
        }
    }

    private static bool TryGetCachedImage(string cacheKey, out BitmapImage image)
    {
        lock (CacheLock)
        {
            if (CachedImages.TryGetValue(cacheKey, out var weakReference) && weakReference.TryGetTarget(out image))
            {
                return true;
            }

            CachedImages.Remove(cacheKey);
        }

        image = null;
        return false;
    }

    private static void CacheImage(string cacheKey, BitmapImage image)
    {
        if (image == null)
        {
            return;
        }

        lock (CacheLock)
        {
            CachedImages[cacheKey] = new WeakReference<BitmapImage>(image);
            CompactCacheIfNeeded();
        }
    }

    private static bool TryMarkAsDownloading(string downloadKey)
    {
        lock (CacheLock)
        {
            return Downloading.Add(downloadKey);
        }
    }

    private static void ClearDownloading(string downloadKey)
    {
        lock (CacheLock)
        {
            Downloading.Remove(downloadKey);
        }
    }

    private static void CompactCacheIfNeeded()
    {
        if (CachedImages.Count < CacheCleanupThreshold)
        {
            return;
        }

        var keysToRemove = new List<string>();
        foreach (var cacheEntry in CachedImages)
        {
            if (!cacheEntry.Value.TryGetTarget(out _))
            {
                keysToRemove.Add(cacheEntry.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            CachedImages.Remove(key);
        }
    }

    #endregion
}