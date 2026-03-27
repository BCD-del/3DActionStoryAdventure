using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilites.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders;
using Assets._Project.Develop.Runtime.Utilites.DataManagment;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System.Collections.Generic;
using System.Linq;

namespace Assets._Project.Develop.Runtime.Meta.Features.StatsUpgrade
{
    public class StatsUpgradeService : IDataReader<PlayerData>, IDataWriter<PlayerData>
    {
        private ConfigsProviderService _configsProviderService;
        private Dictionary<StatTypes, ReactiveVariable<int>> _statLevels = new();

        public StatsUpgradeService(PlayerDataProvider playerDataProvider, ConfigsProviderService configsProviderService)
        {
            _configsProviderService = configsProviderService;
            playerDataProvider.RegisterReader(this);
            playerDataProvider.RegisterWriter(this);
        }

        public List<StatTypes> UpgradeableStats => _statLevels.Keys.Where(t => IsStatUpgradeable(t)).ToList();

        public List<StatTypes> AvailableStats => _statLevels.Keys.ToList();

        public IReadOnlyVariable<int> GetStatLevelFor(StatTypes statType)
            => _statLevels[statType];

        public bool IsStatUpgradeable(StatTypes type)
            => PlayerStatsByLevelConfig.StatConfigs.Any(s => s.Type == type);

        private PlayerStatsUpgradeConfig PlayerStatsByLevelConfig
            => _configsProviderService.GetConfig<PlayerStatsUpgradeConfig>();

        public void InitializeDefaults()
        {
            foreach (StatUpgradeCostConfig statConfig in PlayerStatsByLevelConfig.StatConfigs)
            {
                if (!_statLevels.ContainsKey(statConfig.Type))
                    _statLevels.Add(statConfig.Type, new ReactiveVariable<int>(1));
            }
        }

        public float GetCurrentStatValueFor(StatTypes type)
        {
            return PlayerStatsByLevelConfig.GetStatConfig(type).StatValues[_statLevels[type].Value - 1];
        }

        public CurrencyTypes GetUpgradeCostTypeFor(StatTypes type)
        {
            return PlayerStatsByLevelConfig.GetStatConfig(type).CostType;
        }

        public bool TryGetStatValueForNextLevel(StatTypes type, out float statValue)
        {
            StatUpgradeCostConfig statData = PlayerStatsByLevelConfig.GetStatConfig(type);
            if (statData.StatValues.Count <= _statLevels[type].Value)
            {
                statValue = 0;
                return false;
            }
            statValue = statData.StatValues[_statLevels[type].Value];
            return true;
        }

        public bool TryGetUpgradeCostFor(StatTypes type, out CurrencyTypes costType, out int cost)
        {
            StatUpgradeCostConfig statData = PlayerStatsByLevelConfig.GetStatConfig(type);
            if (statData.UpgradeToNextLevelCost.Count <= _statLevels[type].Value - 1)
            {
                costType = default;
                cost = 0;
                return false;
            }
            costType = statData.CostType;
            cost = statData.UpgradeToNextLevelCost[_statLevels[type].Value - 1];
            return true;
        }

        public bool TryUpgradeStat(StatTypes type)
        {
            StatUpgradeCostConfig statData = PlayerStatsByLevelConfig.GetStatConfig(type);
            if (statData.StatValues.Count <= _statLevels[type].Value)
                return false;
            _statLevels[type].Value += 1;
            return true;
        }

        public void ReadFrom(PlayerData data)
        {
            foreach (var statLevel in data.StatsUpgradeLevel)
            {
                if (_statLevels.ContainsKey(statLevel.Key))
                    _statLevels[statLevel.Key].Value = statLevel.Value;
                else
                    _statLevels.Add(statLevel.Key, new ReactiveVariable<int>(statLevel.Value));
            }
        }

        public void WriteTo(PlayerData data)
        {
            foreach (var stat in _statLevels)
            {
                if (data.StatsUpgradeLevel.ContainsKey(stat.Key))
                    data.StatsUpgradeLevel[stat.Key] = stat.Value.Value;
                else
                    data.StatsUpgradeLevel.Add(stat.Key, stat.Value.Value);
            }
        }
    }
}