using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class EntityController
    {
        private readonly TrackingController _trackingController;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ConcurrentDictionary<long, GameObject> _knownEntities = new ConcurrentDictionary<long, GameObject>();

        public EntityController(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public void AddEntity(long objectId, string name, GameObjectType objectType, GameObjectSubType objectSubType)
        {
            if (_knownEntities.ContainsKey(objectId))
            {
                return;
            }

            var gameObject = new GameObject(objectId)
            {
                Name = name,
                ObjectType = objectType,
                ObjectSubType = objectSubType
            };

            _knownEntities.TryAdd(objectId, gameObject);
            OnAddEntity?.Invoke(gameObject);
        }

        public void RemoveAll()
        {
            _knownEntities.Clear();
        }

        public IEnumerable<GameObject> GetEntities()
        {
            return new List<GameObject>(_knownEntities.Values);
        }

        public event Action<GameObject> OnAddEntity;

        public void HealthUpdate(
            long objectId,
            GameTimeStamp packageTimeStamp,
            float packageHealthChange,
            float packageNewHealthValue,
            EffectType packageEffectType,
            EffectOrigin packageEffectOrigin,
            long packageCauserId,
            int packageCausingSpellType
        )
        {
            OnHealthUpdate?.Invoke(
                objectId,
                packageTimeStamp,
                packageHealthChange,
                packageNewHealthValue,
                packageEffectType,
                packageEffectOrigin,
                packageCauserId,
                packageCausingSpellType
            );
        }

        public event Action<long, GameTimeStamp, float, float, EffectType, EffectOrigin, long, int> OnHealthUpdate;
    }
}