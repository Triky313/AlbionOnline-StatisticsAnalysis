using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.GameFileData;

public static class MobAvatarImageProvider
{
    private const string DefaultAvatarFileName = "p_questgiver_client.png";
    private static readonly string AvatarResourceBasePath = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/Assets/MobAvatars/";
    private static readonly ConcurrentDictionary<string, BitmapImage> AvatarCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, string> AvatarFileNameCache = new(StringComparer.OrdinalIgnoreCase);

    public static BitmapImage GetAvatarSource(string avatar)
    {
        var avatarCacheKey = string.IsNullOrWhiteSpace(avatar) ? DefaultAvatarFileName : avatar;
        var avatarFileName = AvatarFileNameCache.GetOrAdd(
            avatarCacheKey,
            GetExistingAvatarFileName);
        if (AvatarCache.TryGetValue(avatarFileName, out var cachedAvatar))
        {
            return cachedAvatar;
        }

        var avatarSource = CreateAvatarSource(avatarFileName);
        if (avatarSource != null)
        {
            AvatarCache.TryAdd(avatarFileName, avatarSource);
        }

        return avatarSource;
    }

    private static string GetExistingAvatarFileName(string avatar)
    {
        return AvatarResourceExists(avatar)
            ? avatar
            : DefaultAvatarFileName;
    }

    private static bool AvatarResourceExists(string avatar)
    {
        try
        {
            var resourceStream = Application.GetResourceStream(CreateAvatarUri(avatar));
            if (resourceStream == null)
            {
                return false;
            }

            resourceStream.Stream?.Dispose();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static BitmapImage CreateAvatarSource(string avatar)
    {
        try
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmapImage.UriSource = CreateAvatarUri(avatar);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
        catch
        {
            return null;
        }
    }

    private static Uri CreateAvatarUri(string avatar)
    {
        return new Uri($"{AvatarResourceBasePath}{avatar}", UriKind.Absolute);
    }
}