using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldMobKillStatsEntry : BaseViewModel
{
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
        if (string.IsNullOrWhiteSpace(avatar))
        {
            return null;
        }

        if (AvatarCache.TryGetValue(avatar, out var cachedAvatar))
        {
            return cachedAvatar;
        }

        var avatarSource = CreateAvatarSource(avatar);
        if (avatarSource != null)
        {
            AvatarCache.TryAdd(avatar, avatarSource);
        }

        return avatarSource;
    }

    private static BitmapImage CreateAvatarSource(string avatar)
    {
        try
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmapImage.UriSource = new Uri($"{AvatarResourceBasePath}{avatar}", UriKind.Absolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
        catch
        {
            return null;
        }
    }
}
