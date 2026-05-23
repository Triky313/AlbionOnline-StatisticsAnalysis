using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldMobKillStatsEntry : BaseViewModel
{
    private const string DefaultAvatarFileName = "p_questgiver_client.png";
    private static readonly string AvatarResourceBasePath = $"pack://application:,,,/{Assembly.GetExecutingAssembly().GetName().Name};component/Assets/MobAvatars/";
    private static readonly ConcurrentDictionary<string, BitmapImage> AvatarCache = new(StringComparer.OrdinalIgnoreCase);
    private int _kills;
    private double _killsPerHour;

    public string MobUniqueName { get; init; } = string.Empty;
    public string MobName { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public BitmapImage AvatarSource => GetAvatarSource(Avatar);

    public int Kills
    {
        get => _kills;
        set
        {
            _kills = value;
            OnPropertyChanged();
        }
    }

    public double KillsPerHour
    {
        get => _killsPerHour;
        set
        {
            _killsPerHour = value;
            OnPropertyChanged();
        }
    }

    private static BitmapImage GetAvatarSource(string avatar)
    {
        var avatarFileName = GetExistingAvatarFileName(avatar);

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
        if (!string.IsNullOrWhiteSpace(avatar) && AvatarResourceExists(avatar))
        {
            return avatar;
        }

        return DefaultAvatarFileName;
    }

    private static bool AvatarResourceExists(string avatar)
    {
        try
        {
            return Application.GetResourceStream(CreateAvatarUri(avatar)) != null;
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