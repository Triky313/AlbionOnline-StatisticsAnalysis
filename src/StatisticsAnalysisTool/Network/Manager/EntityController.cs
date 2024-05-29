using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager;

public class EntityController
{
    private readonly ConcurrentDictionary<Guid, PlayerGameObject> _knownEntities = new();
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ObservableCollection<EquipmentItemInternal> _newEquipmentItems = new();
    private readonly ObservableCollection<SpellEffect> _spellEffects = new();
    private readonly ConcurrentDictionary<long, CharacterEquipmentData> _tempCharacterEquipmentData = new();
    private double _lastLocalEntityGuildTaxInPercent;
    private double _lastLocalEntityClusterTaxInPercent;
    private readonly TrackingController _trackingController;

    public LocalUserData LocalUserData { get; init; } = new();

    public EntityController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    #region Entities

    public void AddEntity(Entity entity)
    {
        PlayerGameObject gameObject;

        if (_knownEntities.TryRemove(entity.UserGuid, out var oldEntity))
        {
            // Parties are recreated several times in HCE's and therefore the ObjectId may only be set to zero once after a map change.
            // However, this must not happen in AddEntity
            long? newUserObjectId = oldEntity.ObjectId;
            if (entity.ObjectId != null)
            {
                newUserObjectId = entity.ObjectId;
            }

            gameObject = new PlayerGameObject(newUserObjectId)
            {
                Name = entity.Name,
                ObjectType = entity.ObjectType,
                UserGuid = entity.UserGuid,
                Guild = string.Empty == entity.Guild ? oldEntity.Guild : entity.Guild,
                Alliance = string.Empty == entity.Alliance ? oldEntity.Alliance : entity.Alliance,
                InteractGuid = entity.InteractGuid == Guid.Empty || entity.InteractGuid == null ? oldEntity.InteractGuid : entity.InteractGuid,
                ObjectSubType = entity.ObjectSubType,
                CharacterEquipment = entity.CharacterEquipment ?? oldEntity.CharacterEquipment,
                CombatStart = oldEntity.CombatStart,
                CombatTime = oldEntity.CombatTime,
                Damage = oldEntity.Damage,
                Heal = oldEntity.Heal,
                Overhealed = oldEntity.Overhealed,
                IsInParty = oldEntity.IsInParty,
            };
        }
        else
        {
            gameObject = new PlayerGameObject(entity.ObjectId)
            {
                Name = entity.Name,
                ObjectType = entity.ObjectType,
                UserGuid = entity.UserGuid,
                Guild = entity.Guild,
                Alliance = entity.Alliance,
                ObjectSubType = entity.ObjectSubType,
                CharacterEquipment = entity.CharacterEquipment
            };

            if (gameObject.ObjectSubType == GameObjectSubType.LocalPlayer)
            {
                RemoveLocalEntityFromPartyAsync().GetAwaiter();
            }
        }

        // When players in a group and go to the Mist, the party player is indicated as PA
        if (gameObject.Name == "PA" && oldEntity?.Name != null)
        {
            gameObject.Name = oldEntity.Name;
        }

        if (entity.ObjectId is not null && _tempCharacterEquipmentData.TryGetValue((long) entity.ObjectId, out var characterEquipmentData))
        {
            ResetTempCharacterEquipment();
            gameObject.CharacterEquipment = characterEquipmentData.CharacterEquipment;
            _tempCharacterEquipmentData.TryRemove((long) entity.ObjectId, out _);
        }

        _knownEntities.TryAdd(gameObject.UserGuid, gameObject);
    }

    public void RemoveEntitiesByLastUpdate(int withoutAnUpdateForMinutes)
    {
        foreach (var entity in _knownEntities.Where(x =>
                     x.Value.ObjectSubType != GameObjectSubType.LocalPlayer
                     && x.Value.Damage <= 0
                     && !IsEntityInParty(x.Key)
                     && new DateTime(x.Value.LastUpdate).AddMinutes(withoutAnUpdateForMinutes).Ticks < DateTime.UtcNow.Ticks))
        {
            _knownEntities.TryRemove(entity.Key, out _);
        }

        foreach (var entity in _knownEntities.Where(x => x.Value.ObjectSubType != GameObjectSubType.LocalPlayer))
        {
            entity.Value.ObjectId = null;
        }
    }

    public KeyValuePair<Guid, PlayerGameObject>? GetEntity(long objectId)
    {
        return _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
    }

    public KeyValuePair<Guid, PlayerGameObject>? GetEntity(string uniqueName)
    {
        return _knownEntities?.FirstOrDefault(x => x.Value.Name == uniqueName);
    }

    public KeyValuePair<Guid, PlayerGameObject> GetEntity(Guid guid)
    {
        return _knownEntities.FirstOrDefault(x => x.Key == guid);
    }

    public List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntities(bool onlyInParty = false)
    {
        return new List<KeyValuePair<Guid, PlayerGameObject>>(onlyInParty ? _knownEntities.ToArray().Where(x => IsEntityInParty(x.Value.Name)) : _knownEntities.ToArray());
    }

    public List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntitiesWithDamageOrHealAndInParty()
    {
        return new List<KeyValuePair<Guid, PlayerGameObject>>(_knownEntities
            .ToArray()
            .Where(x => (x.Value.Damage > 0 || x.Value.Heal > 0 || x.Value.Overhealed > 0) && IsEntityInParty(x.Key)));
    }

    public bool ExistEntity(Guid guid)
    {
        return _knownEntities?.Any(x => x.Key == guid) ?? false;
    }

    public void SetItemPower(Guid guid, double itemPower)
    {
        var entity = GetEntity(guid);

        if (entity.Value == null)
        {
            return;
        }

        if (Math.Abs(entity.Value.ItemPower - itemPower) > 0)
        {
            entity.Value.ItemPower = itemPower;
        }
    }

    #endregion

    #region Party

    public async Task AddToPartyAsync(Guid guid)
    {
        var entity = GetEntity(guid);
        if (entity.Value is { IsInParty: false })
        {
            entity.Value.IsInParty = true;
            await SetPartyMemberUiAsync();
            await _trackingController?.PartyController?.UpdatePartyAsync()!;
        }
    }

    private async Task RemoveLocalEntityFromPartyAsync()
    {
        var entity = GetLocalEntity();
        if (entity?.Value is not null)
        {
            await RemoveFromPartyAsync(entity.Value.Key);
        }
    }

    public async Task RemoveFromPartyAsync(Guid? guid)
    {
        if (guid is { } notNullGuid)
        {
            if (notNullGuid == GetLocalEntity()?.Key)
            {
                await ResetPartyMemberAsync();
                await AddLocalEntityToPartyAsync();
            }
            else
            {
                var entity = GetEntity(notNullGuid);
                if (entity.Value != null)
                {
                    entity.Value.IsInParty = false;
                }
            }

            await SetPartyMemberUiAsync();
            await _trackingController?.PartyController?.UpdatePartyAsync()!;
        }
    }

    public async Task ResetPartyMemberAsync()
    {
        foreach (var partyEntities in _knownEntities.Where(x => x.Value.IsInParty))
        {
            partyEntities.Value.IsInParty = false;
        }

        await SetPartyMemberUiAsync();
        await _trackingController?.PartyController?.UpdatePartyAsync()!;
    }

    public async Task AddLocalEntityToPartyAsync()
    {
        var localEntity = GetLocalEntity();
        if (localEntity != null)
        {
            localEntity.Value.Value.IsInParty = true;
            await SetPartyMemberUiAsync();
            await _trackingController?.PartyController?.UpdatePartyAsync()!;
        }
    }

    public async Task SetPartyAsync(Dictionary<Guid, string> party)
    {
        await ResetPartyMemberAsync();

        foreach (var member in party)
        {
            if (!ExistEntity(member.Key) && GetLocalEntity()?.Key != member.Key)
            {
                AddEntity(new Entity
                {
                    UserGuid = member.Key,
                    Name = member.Value,
                    ObjectType = GameObjectType.Player,
                    ObjectSubType = GameObjectSubType.Player
                });
            }

            await AddToPartyAsync(member.Key);
        }

        await SetPartyMemberUiAsync();
        await _trackingController?.PartyController?.UpdatePartyAsync()!;
    }

    private async Task SetPartyMemberUiAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            _mainWindowViewModel.PartyMemberCircles.Clear();

            var localEntity = GetLocalEntity();
            if (localEntity != null)
            {
                _mainWindowViewModel.PartyMemberCircles.Add(new PartyMemberCircle
                {
                    Name = localEntity.Value.Value?.Name,
                    UserGuid = localEntity.Value.Key
                });
            }

            foreach (var member in _knownEntities.Where(x => x.Value.IsInParty && x.Key != localEntity?.Value?.UserGuid).ToList())
            {
                _mainWindowViewModel.PartyMemberCircles.Add(new PartyMemberCircle
                {
                    Name = member.Value.Name,
                    UserGuid = member.Key
                });
            }

            _mainWindowViewModel.PartyMemberNumber = _knownEntities.Count(x => x.Value.IsInParty);
        });
    }

    public bool IsEntityInParty(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        return _knownEntities?.FirstOrDefault(x => x.Value?.Name == name).Value?.IsInParty ?? false;
    }

    public bool IsEntityInParty(long objectId)
    {
        var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        return IsEntityInParty(entity?.Value?.Name);
    }

    public bool IsEntityInParty(Guid guid)
    {
        return _knownEntities?.FirstOrDefault(x => x.Key == guid).Value?.IsInParty ?? false;
    }

    public bool IsAnyEntityInParty(List<Guid> guids)
    {
        return _knownEntities?.Any(x => guids.Contains(x.Key) && x.Value.IsInParty) ?? false;
    }

    public void CopyPartyToClipboard()
    {
        var output = string.Empty;
        var partyString = _knownEntities.Where(x => x.Value.IsInParty)
            .Aggregate(output, (current, entity) => current + $"{entity.Value.Name},");

        Clipboard.SetDataObject(partyString[..(partyString.Length > 0 ? partyString.Length - 1 : 0)]);
    }

    #endregion

    #region Equipment

    public async Task SetCharacterEquipmentAsync(long objectId, CharacterEquipment equipment)
    {
        var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        if (entity?.Value != null)
        {
            entity.Value.Value.CharacterEquipment = equipment;

            if (entity.Value.Value.IsInParty)
            {
                await _trackingController?.PartyController?.UpdatePartyAsync()!;
            }
        }
        else
        {
            _tempCharacterEquipmentData.TryAdd(objectId, new CharacterEquipmentData
            {
                CharacterEquipment = equipment,
                TimeStamp = DateTime.UtcNow
            });
        }
    }

    public void ResetTempCharacterEquipment()
    {
        foreach (var characterEquipment in _tempCharacterEquipmentData.ToArray())
        {
            if (Utilities.IsBlockingTimeExpired(characterEquipment.Value.TimeStamp, 30))
            {
                _tempCharacterEquipmentData.TryRemove(characterEquipment.Key, out _);
            }
        }
    }

    public void AddEquipmentItem(EquipmentItemInternal itemInternal)
    {
        lock (_newEquipmentItems)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_newEquipmentItems.ToList().Any(x => x == null || x.ItemIndex.Equals(itemInternal?.ItemIndex) && x.SpellDictionary?.Values == itemInternal.SpellDictionary?.Values))
                {
                    return;
                }

                _newEquipmentItems.Add(itemInternal);
            });
        }

        RemoveSpellAndEquipmentObjects();
    }

    public void AddSpellEffect(SpellEffect spell)
    {
        if (!IsEntityInParty(spell.CauserId))
        {
            return;
        }

        lock (_spellEffects)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_spellEffects.Any(x => x == null || (x.CauserId.Equals(spell.CauserId) && x.SpellIndex.Equals(spell.SpellIndex))))
                {
                    return;
                }

                _spellEffects.Add(spell);
            });
        }

        RemoveSpellAndEquipmentObjects();
    }

    public void DetectUsedWeapon()
    {
        var playerItemList = new Dictionary<long, int>();

        try
        {
            lock (_newEquipmentItems)
            {
                foreach (var item in _newEquipmentItems.ToList())
                {
                    foreach (var spell in
                             (from itemSpell in item.SpellDictionary.ToArray()
                              from spell in _spellEffects.ToArray()
                              where spell != null && spell.SpellIndex.Equals(itemSpell.Value)
                              select spell).ToArray())
                    {
                        if (playerItemList.Any(x => x.Key.Equals(spell.CauserId)))
                        {
                            continue;
                        }

                        playerItemList.Add(spell.CauserId, item.ItemIndex);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }

        foreach (var (key, value) in playerItemList.ToList())
        {
            SetCharacterMainHand(key, value);
        }
    }

    private void SetCharacterMainHand(long objectId, int itemIndex)
    {
        var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        var entityValue = entity?.Value;

        if (entityValue == null)
        {
            return;
        }

        entityValue.CharacterEquipment ??= new CharacterEquipment
        {
            MainHand = itemIndex
        };
    }

    private void RemoveSpellAndEquipmentObjects()
    {
        lock (_newEquipmentItems)
        {
            foreach (var item in _newEquipmentItems.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-20)))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _newEquipmentItems.Remove(item);
                });
            }
        }

        lock (_spellEffects)
        {
            foreach (var spell in _spellEffects.ToList().Where(x => x?.TimeStamp < DateTime.UtcNow.AddSeconds(-20)))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _spellEffects.Remove(spell);
                });
            }
        }
    }

    public class CharacterEquipmentData
    {
        public DateTime TimeStamp { get; set; }
        public CharacterEquipment CharacterEquipment { get; set; }
    }

    #endregion

    #region Damage

    public void ResetEntitiesDamageStartTime()
    {
        foreach (var entity in _knownEntities)
        {
            entity.Value.CombatStart = null;
        }
    }

    public void ResetEntitiesDamageTimes()
    {
        foreach (var entity in _knownEntities)
        {
            entity.Value.ResetCombatTimes();
        }
    }

    public void ResetEntitiesDamage()
    {
        foreach (var entity in _knownEntities)
        {
            entity.Value.Damage = 0;
        }
    }

    public void ResetEntitiesHeal()
    {
        foreach (var entity in _knownEntities)
        {
            entity.Value.Heal = 0;
        }
    }

    public void ResetEntitiesHealAndOverhealed()
    {
        foreach (var entity in _knownEntities)
        {
            entity.Value.Overhealed = 0;
        }
    }

    #endregion

    #region Health

    public void HealthUpdate(
        long objectId,
        GameTimeStamp timeStamp,
        double healthChange,
        double newHealthValue,
        EffectType effectType,
        EffectOrigin effectOrigin,
        long causerId,
        int causingSpellType
    )
    {
        OnHealthUpdate?.Invoke(
            objectId,
            timeStamp,
            healthChange,
            newHealthValue,
            effectType,
            effectOrigin,
            causerId,
            causingSpellType
        );
    }

    public event Action<long, GameTimeStamp, double, double, EffectType, EffectOrigin, long, int> OnHealthUpdate;

    #endregion

    #region Local Entity

    public FixPoint GetLastLocalEntityClusterTax(FixPoint yieldPreClusterTax) => FixPoint.FromFloatingPointValue(yieldPreClusterTax.DoubleValue / 100 * _lastLocalEntityClusterTaxInPercent);

    public void SetLastLocalEntityClusterTax(FixPoint yieldPreTax, FixPoint clusterTax)
    {
        _lastLocalEntityClusterTaxInPercent = (100 / yieldPreTax.DoubleValue) * clusterTax.DoubleValue;
    }

    public void SetLastLocalEntityGuildTax(FixPoint yieldPreTax, FixPoint guildTax)
    {
        _lastLocalEntityGuildTaxInPercent = (100 / yieldPreTax.DoubleValue) * guildTax.DoubleValue;
    }

    public FixPoint GetLastLocalEntityGuildTax(FixPoint yieldPreTax) => FixPoint.FromFloatingPointValue(yieldPreTax.DoubleValue / 100 * _lastLocalEntityGuildTaxInPercent);

    public bool ExistLocalEntity()
    {
        return _knownEntities?.Any(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer) ?? false;
    }

    public KeyValuePair<Guid, PlayerGameObject>? GetLocalEntity() => _knownEntities?.ToArray().FirstOrDefault(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer);

    #endregion
}