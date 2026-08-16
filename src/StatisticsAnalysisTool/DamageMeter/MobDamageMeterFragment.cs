using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class MobDamageMeterFragment : BaseViewModel
{
    private Visibility _playersContainerVisibility = Visibility.Collapsed;
    private ObservableCollection<DamageMeterFragment> _players = [];
    private long _damage;
    private double _damagePercentage;
    private TimeSpan _combatTime;
    private double _dps;

    public Guid MobInstanceId { get; init; }
    public long MobObjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string UniqueName { get; init; } = string.Empty;
    public string ClusterName { get; init; } = string.Empty;
    public BitmapImage AvatarSource { get; init; }

    public long Damage
    {
        get => _damage;
        internal set
        {
            if (_damage == value)
            {
                return;
            }

            _damage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DamageShortString));
        }
    }

    public string DamageShortString => Damage.ToShortNumberString();

    public double DamagePercentage
    {
        get => _damagePercentage;
        internal set
        {
            if (_damagePercentage.Equals(value))
            {
                return;
            }

            _damagePercentage = value;
            OnPropertyChanged();
        }
    }

    public DateTime FirstAttackTime { get; init; }
    public DateTime FirstAttackTimeLocal => FirstAttackTime.ToLocalTime();

    public TimeSpan CombatTime
    {
        get => _combatTime;
        internal set
        {
            if (_combatTime == value)
            {
                return;
            }

            _combatTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CombatTimeString));
        }
    }

    public string CombatTimeString => $"{(int) CombatTime.TotalHours:00}:{CombatTime.Minutes:00}:{CombatTime.Seconds:00}";

    public double Dps
    {
        get => _dps;
        internal set
        {
            if (_dps.Equals(value))
            {
                return;
            }

            _dps = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DpsShortString));
        }
    }

    public string DpsShortString => Dps.ToShortNumberString();
    public Tier MobTier { get; init; } = Tier.Unknown;
    public string MobTierString => (int) MobTier is >= 1 and <= 8 ? MobTier.ToString() : "T?";
    public string MobType { get; init; } = string.Empty;
    public MobRankCategory MobRankCategory => MobRankCategoryResolver.Resolve(UniqueName);
    public string MobRank => MobRankCategoryResolver.GetDisplayName(MobRankCategory);
    public DashboardContentType ContentType { get; init; }
    public string ContentTypeName => LocalizationController.Translation(DashboardContentTypeResolver.GetTranslationKey(ContentType));
    public string MapName { get; init; } = string.Empty;
    public Tier MapTier { get; init; } = Tier.Unknown;
    public string MapTierString => MapTier == Tier.Unknown ? "T?" : MapTier.ToString();

    public ObservableCollection<DamageMeterFragment> Players
    {
        get => _players;
        init => _players = value;
    }

    public Visibility PlayersContainerVisibility
    {
        get => _playersContainerVisibility;
        set
        {
            _playersContainerVisibility = value;
            OnPropertyChanged();
        }
    }

    public int PlayerCount => Players.Count;

    internal void UpdateFrom(MobDamageMeterFragment updatedFragment)
    {
        var damageChanged = Damage != updatedFragment.Damage;
        DamagePercentage = updatedFragment.DamagePercentage;

        if (!damageChanged)
        {
            return;
        }

        Damage = updatedFragment.Damage;
        CombatTime = updatedFragment.CombatTime;
        Dps = updatedFragment.Dps;
        SynchronizePlayers(updatedFragment.Players);
    }

    private void SynchronizePlayers(IReadOnlyCollection<DamageMeterFragment> updatedPlayers)
    {
        var orderedPlayers = updatedPlayers.ToList();
        var updatedPlayerIds = orderedPlayers
            .Select(x => x.CauserGuid)
            .ToHashSet();
        var playerCountChanged = Players.Count != orderedPlayers.Count;

        for (var index = Players.Count - 1; index >= 0; index--)
        {
            if (!updatedPlayerIds.Contains(Players[index].CauserGuid))
            {
                Players.RemoveAt(index);
            }
        }

        var existingPlayers = Players.ToDictionary(x => x.CauserGuid);
        for (var targetIndex = 0; targetIndex < orderedPlayers.Count; targetIndex++)
        {
            var updatedPlayer = orderedPlayers[targetIndex];
            if (!existingPlayers.TryGetValue(updatedPlayer.CauserGuid, out var existingPlayer))
            {
                Players.Insert(targetIndex, updatedPlayer);
                existingPlayers.Add(updatedPlayer.CauserGuid, updatedPlayer);
                continue;
            }

            UpdatePlayer(existingPlayer, updatedPlayer);
            var currentIndex = Players.IndexOf(existingPlayer);
            if (currentIndex != targetIndex)
            {
                Players.Move(currentIndex, targetIndex);
            }
        }

        if (playerCountChanged)
        {
            OnPropertyChanged(nameof(PlayerCount));
        }
    }

    private static void UpdatePlayer(DamageMeterFragment player, DamageMeterFragment updatedPlayer)
    {
        if (player.Name != updatedPlayer.Name)
        {
            player.Name = updatedPlayer.Name;
        }

        if (!Equals(player.CauserMainHand, updatedPlayer.CauserMainHand))
        {
            player.CauserMainHand = updatedPlayer.CauserMainHand;
        }

        if (player.Damage != updatedPlayer.Damage)
        {
            player.Damage = updatedPlayer.Damage;
        }

        if (!player.DamageInPercent.Equals(updatedPlayer.DamageInPercent))
        {
            player.DamageInPercent = updatedPlayer.DamageInPercent;
        }

        if (!player.DamagePercentage.Equals(updatedPlayer.DamagePercentage))
        {
            player.DamagePercentage = updatedPlayer.DamagePercentage;
        }

        if (!player.Dps.Equals(updatedPlayer.Dps))
        {
            player.Dps = updatedPlayer.Dps;
        }

        if (player.CombatTime != updatedPlayer.CombatTime)
        {
            player.CombatTime = updatedPlayer.CombatTime;
        }

        SynchronizeSpells(player.Spells, updatedPlayer.Spells);
    }

    private static void SynchronizeSpells(
        ObservableCollection<UsedSpellFragment> spells,
        IReadOnlyCollection<UsedSpellFragment> updatedSpells)
    {
        var orderedSpells = updatedSpells.ToList();
        var updatedSpellIndexes = orderedSpells
            .Select(x => x.SpellIndex)
            .ToHashSet();

        for (var index = spells.Count - 1; index >= 0; index--)
        {
            if (!updatedSpellIndexes.Contains(spells[index].SpellIndex))
            {
                spells.RemoveAt(index);
            }
        }

        var existingSpells = spells.ToDictionary(x => x.SpellIndex);
        for (var targetIndex = 0; targetIndex < orderedSpells.Count; targetIndex++)
        {
            var updatedSpell = orderedSpells[targetIndex];
            if (!existingSpells.TryGetValue(updatedSpell.SpellIndex, out var existingSpell))
            {
                spells.Insert(targetIndex, updatedSpell);
                existingSpells.Add(updatedSpell.SpellIndex, updatedSpell);
                continue;
            }

            UpdateSpell(existingSpell, updatedSpell);
            var currentIndex = spells.IndexOf(existingSpell);
            if (currentIndex != targetIndex)
            {
                spells.Move(currentIndex, targetIndex);
            }
        }
    }

    private static void UpdateSpell(UsedSpellFragment spell, UsedSpellFragment updatedSpell)
    {
        if (spell.ItemIndex != updatedSpell.ItemIndex)
        {
            spell.ItemIndex = updatedSpell.ItemIndex;
        }

        if (spell.UniqueName != updatedSpell.UniqueName)
        {
            spell.UniqueName = updatedSpell.UniqueName;
        }

        if (spell.Target != updatedSpell.Target)
        {
            spell.Target = updatedSpell.Target;
        }

        if (spell.Category != updatedSpell.Category)
        {
            spell.Category = updatedSpell.Category;
        }

        if (spell.DamageHealValue != updatedSpell.DamageHealValue)
        {
            spell.DamageHealValue = updatedSpell.DamageHealValue;
        }

        if (!spell.DamageInPercent.Equals(updatedSpell.DamageInPercent))
        {
            spell.DamageInPercent = updatedSpell.DamageInPercent;
        }

        if (!spell.DamagePercentage.Equals(updatedSpell.DamagePercentage))
        {
            spell.DamagePercentage = updatedSpell.DamagePercentage;
        }

        if (spell.HealthChangeType != updatedSpell.HealthChangeType)
        {
            spell.HealthChangeType = updatedSpell.HealthChangeType;
        }

        if (spell.Ticks != updatedSpell.Ticks)
        {
            spell.Ticks = updatedSpell.Ticks;
        }
    }

    private void TogglePlayers(object value)
    {
        PlayersContainerVisibility = PlayersContainerVisibility == Visibility.Visible
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    public static string TranslationFirstAttackTime => LocalizationController.Translation("FIRST_ATTACK_TIME");
    public static string TranslationCombatTime => LocalizationController.Translation("COMBAT_TIME");
    public static string TranslationPlayers => LocalizationController.Translation("PLAYERS");
    public static string TranslationLocation => LocalizationController.Translation("LOCATION");

    private ICommand _showPlayers;
    public ICommand ShowPlayers => _showPlayers ??= new CommandHandler(TogglePlayers, true);
}