using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class CombatController
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly DamageChartController _damageChartController;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ObservableCollection<DamageObject> _damageCollection = new ObservableCollection<DamageObject>();
        private readonly ObservableCollection<DateTime> _clusterStarts = new ObservableCollection<DateTime>();

        public CombatController(MainWindowViewModel mainWindowViewModel, DamageChartController damageChartController)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _damageChartController = damageChartController;
        }

        public void SetDamageValuesToDamageMeter()
        {
            if (_clusterStarts.Count <= 0)
            {
                AddClusterStartTimer();
            }

            // TODO: ObjectId dem Namen zuweisen, anhand von PartyController

            GetDamageListByNewestCluster().GroupBy(x => x.CauserId).ToList().ForEach(damageObject =>
            {
                if (damageObject != null)
                {
                    _mainWindowViewModel.SetDamageObjectToDamageMeter(new DamageMeterObject { Name = damageObject.FirstOrDefault()?.CauserId.ToString(), Value = damageObject.Sum(x => x.Damage) });
                }
            });
        }

        public void AddClusterStartTimer()
        {
            _clusterStarts.Add(DateTime.UtcNow);
        }

        public void AddDamage(long objectId, long causerId, double healthChange)
        {
            var damageValue = (int)Math.Round(healthChange.ToPositive(), MidpointRounding.AwayFromZero);
            _damageCollection.Add(new DamageObject(causerId, objectId, damageValue));
            SetDamageValuesToDamageMeter();
        }
        
        public void RemoveAll()
        {
            _damageCollection.Clear();
            _clusterStarts.Clear();
        }

        private List<DamageObject> GetDamageListByNewestCluster()
        {
            if (_clusterStarts?.GetHighestDateTime() == null)
            {
                return new List<DamageObject>();
            }

            return _damageCollection.Where(x => x.TimeStamp >= _clusterStarts.GetHighestDateTime()).ToList();
        }
    }
}