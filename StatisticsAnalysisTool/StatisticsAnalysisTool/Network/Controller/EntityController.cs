using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class EntityController
    {
        private readonly ConcurrentDictionary<Guid, GameObject> _knownEntities = new ConcurrentDictionary<Guid, GameObject>();
        private readonly ConcurrentDictionary<Guid, string> _knownPartyEntities = new ConcurrentDictionary<Guid, string>();

        public void AddEntity(long objectId, Guid userGuid, string name, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            var gameObject = new GameObject(objectId)
            {
                Name = name,
                ObjectType = objectType,
                UserGuid = userGuid,
                ObjectSubType = objectSubType,
            };

            _knownEntities.TryRemove(userGuid, out _);
            _knownEntities.TryAdd(gameObject.UserGuid, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveAllEntities()
        {
            foreach (var entity in _knownEntities.Where(x => x.Value.ObjectSubType != GameObjectSubType.LocalPlayer && !_knownPartyEntities.ContainsKey(x.Key)))
            {
                _knownEntities.TryRemove(entity.Key, out _);
            }

            foreach (var entity in _knownEntities.Where(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer || _knownPartyEntities.ContainsKey(x.Key)))
            {
                entity.Value.ObjectId = null;
            }
        }

        public void ResetPartyMember()
        {
            _knownPartyEntities.Clear();

            foreach (var member in _knownEntities.Where(x => x.Value.ObjectSubType == GameObjectSubType.LocalPlayer))
            {
                _knownPartyEntities.TryAdd(member.Key, member.Value.Name);
            }
        }

        public void AddToParty(Guid guid, string username)
        {
            if (_knownPartyEntities.All(x => x.Key != guid))
            {
                _knownPartyEntities.TryAdd(guid, username);
            }
        }

        public void SetParty(Dictionary<Guid, string> party, bool resetPartyBefore = false)
        {
            if (resetPartyBefore)
            {
                ResetPartyMember();
            }

            foreach (var member in party)
            {
                AddToParty(member.Key, member.Value);
            }
        }

        public bool IsUserInParty(string name)
        {
            return _knownPartyEntities.Any(x => x.Value == name);
        }

        public KeyValuePair<Guid, GameObject>? GetEntity(long objectId)
        {
            return _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
        }

        public void SetCharacterEquipment(long objectId, CharacterEquipment equipment)
        {
            var entity = _knownEntities?.FirstOrDefault(x => x.Value.ObjectId == objectId);
            if (entity?.Value != null)
            {
                entity.Value.Value.CharacterEquipment = equipment;
            }
        }

        public event Action<GameObject> OnAddEntity;

        public void HealthUpdate(
            long objectId,
            GameTimeStamp TimeStamp,
            double HealthChange,
            double NewHealthValue,
            EffectType EffectType,
            EffectOrigin EffectOrigin,
            long CauserId,
            int CausingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                TimeStamp,
                HealthChange,
                NewHealthValue,
                EffectType,
                EffectOrigin,
                CauserId,
                CausingSpellType
            );
        }

        public event Action<long, GameTimeStamp, double, double, EffectType, EffectOrigin, long, int> OnHealthUpdate;
    }
}