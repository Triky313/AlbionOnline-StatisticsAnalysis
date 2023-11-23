using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;

namespace StatisticsAnalysisTool.Network.Manager;

public interface IEntityController
{
    LocalUserData LocalUserData { get; init; }

    event Action<GameObject> OnAddEntity;
    void AddEntity(long? objectId, Guid userGuid, Guid? interactGuid, string name, string guild, string alliance,
        CharacterEquipment characterEquipment, GameObjectType objectType, GameObjectSubType objectSubType);
    void RemoveEntitiesByLastUpdate(int withoutAnUpdateForMinutes);
    KeyValuePair<Guid, PlayerGameObject>? GetEntity(long objectId);
    KeyValuePair<Guid, PlayerGameObject>? GetEntity(string uniqueName);
    KeyValuePair<Guid, PlayerGameObject> GetEntity(Guid guid);
    List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntities(bool onlyInParty = false);
    List<KeyValuePair<Guid, PlayerGameObject>> GetAllEntitiesWithDamageOrHealAndInParty();
    bool ExistEntity(Guid guid);
    void SetItemPower(Guid guid, double itemPower);

    Task AddToPartyAsync(Guid guid);
    Task RemoveFromPartyAsync(Guid? guid);
    Task ResetPartyMemberAsync();
    Task AddLocalEntityToPartyAsync();
    Task SetPartyAsync(Dictionary<Guid, string> party);
    bool IsEntityInParty(string name);
    bool IsEntityInParty(long objectId);
    bool IsEntityInParty(Guid guid);
    bool IsAnyEntityInParty(List<Guid> guids);
    void CopyPartyToClipboard();

    Task SetCharacterEquipmentAsync(long objectId, CharacterEquipment equipment);
    void ResetTempCharacterEquipment();
    void AddEquipmentItem(EquipmentItemInternal itemInternal);
    void AddSpellEffect(SpellEffect spell);
    void DetectUsedWeapon();

    void ResetEntitiesDamageStartTime();
    void ResetEntitiesDamageTimes();
    void ResetEntitiesDamage();
    void ResetEntitiesHeal();
    void ResetEntitiesHealAndOverhealed();

    void HealthUpdate(
        long objectId,
        GameTimeStamp timeStamp,
        double healthChange,
        double newHealthValue,
        EffectType effectType,
        EffectOrigin effectOrigin,
        long causerId,
        int causingSpellType
    );
    event Action<long, GameTimeStamp, double, double, EffectType, EffectOrigin, long, int> OnHealthUpdate;

    FixPoint GetLastLocalEntityClusterTax(FixPoint yieldPreClusterTax);
    void SetLastLocalEntityClusterTax(FixPoint yieldPreTax, FixPoint clusterTax);
    void SetLastLocalEntityGuildTax(FixPoint yieldPreTax, FixPoint guildTax);
    FixPoint GetLastLocalEntityGuildTax(FixPoint yieldPreTax);
    bool ExistLocalEntity();
    KeyValuePair<Guid, PlayerGameObject>? GetLocalEntity();
}