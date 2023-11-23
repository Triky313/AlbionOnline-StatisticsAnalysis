using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using StatisticsAnalysisTool.DamageMeter;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Manager;

public interface ICombatController
{
    public event Action<ObservableCollection<DamageMeterFragment>, List<KeyValuePair<Guid, PlayerGameObject>>> OnDamageUpdate;

    public Task AddDamage(long affectedId, long causerId, double healthChange, double newHealthValue);
    void UpdateDamageMeterUiAsync(ObservableCollection<DamageMeterFragment> damageMeter,
        List<KeyValuePair<Guid, PlayerGameObject>> entities);
    void ResetDamageMeterByClusterChange();
    void ResetDamageMeterBeforeCombatStart(long objectId, bool inActiveCombat, bool inPassiveCombat);
    void ResetDamageMeter();
    bool IsMaxHealthReached(long objectId, double newHealthValue);
    event Action<long, bool, bool> OnChangeCombatMode;
    void UpdateCombatMode(long objectId, bool inActiveCombat, bool inPassiveCombat);
}