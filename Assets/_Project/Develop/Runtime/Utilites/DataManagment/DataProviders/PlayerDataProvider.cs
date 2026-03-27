using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature;
using Assets._Project.Develop.Runtime.Meta.Features.Wallet;
using Assets._Project.Develop.Runtime.Utilites.ConfigsManagment;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Utilites.DataManagment.DataProviders
{
    public class PlayerDataProvider : DataProvider<PlayerData>
    {
        private readonly ConfigsProviderService _configsProviderService;

        public PlayerDataProvider(
            ISaveLoadService saveLoadSerivce,
            ConfigsProviderService configsProviderService) : base(saveLoadSerivce)
        {
            _configsProviderService = configsProviderService;
        }

        protected override PlayerData GetOriginData()
        {
            return new PlayerData()
            {
                WalletData = InitWalletData(),
                CompletedLevels = new(),
                StatsUpgradeLevel = InintStatsUpgradesLevels()
            };
        }

        private Dictionary<StatTypes, int> InintStatsUpgradesLevels()
        {
            Dictionary<StatTypes, int> statUpgradesLevels = new();
            PlayerStatsUpgradeConfig upgradeConfig = _configsProviderService.GetConfig<PlayerStatsUpgradeConfig>();
            foreach (StatUpgradeCostConfig statConfig in upgradeConfig.StatConfigs)
                statUpgradesLevels.Add(statConfig.Type, 1);
            return statUpgradesLevels;
        }

        private Dictionary<CurrencyTypes, int> InitWalletData()
        {
            Dictionary<CurrencyTypes, int> walletData = new();

            StartWalletConfig walletConfig = _configsProviderService.GetConfig<StartWalletConfig>();

            foreach (CurrencyTypes currencyType in Enum.GetValues(typeof(CurrencyTypes)))
                walletData[currencyType] = walletConfig.GetValueFor(currencyType);

            return walletData;
        }
    }
}