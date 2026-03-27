using Assets._Project.Develop.Runtime.Configs.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Abilities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Loot;
using Assets._Project.Develop.Runtime.Configs.Meta.Stats;
using Assets._Project.Develop.Runtime.Configs.Meta.Wallet;
using Assets._Project.Develop.Runtime.Utilites.AssetsManagment;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilites.ConfigsManagment
{
    public class ResourcesConfigsLoader : IConfigLoader
    {
        private readonly ResourcesAssetsLoader _resources;

        private readonly Dictionary<Type, string> _configsResourcesPath = new()
        {
            { typeof(LootListConfig), "Configs/Gameplay/Loot/LootListConfig" },

            { typeof(StartWalletConfig), "Configs/Meta/Wallet/StartWalletConfig" },
            { typeof(CurrencyIconsConfig), "Configs/Meta/Wallet/CurrencyIconsConfig" },

            { typeof(PlayerStatsUpgradeConfig), "Configs/Meta/Stats/PlayerStatsUpgradeConfig" },
            { typeof(StatsViewConfig), "Configs/Meta/Stats/StatsViewConfig" },

            { typeof(LevelsListConfig), "Configs/Gameplay/Levels/LevelsListConfig" },

            { typeof(HeroConfig), "Configs/Entities/Characters/HeroConfig" },

            { typeof(AbilitiesConfigsContainer), "Configs/Gameplay/Abilities/AbilitiesConfigsContainer" },

            { typeof(ExperienceForUpgradeLevelConfig), "Configs/Gameplay/ExperienceForUpgradeLevelConfig" },
        };

        public ResourcesConfigsLoader(ResourcesAssetsLoader resources)
        {
            _resources = resources;
        }

        public IEnumerator LoadAsync(Action<Dictionary<Type, object>> onConfigsLoaded)
        {
            Dictionary<Type, object> loadedConfigs = new();

            foreach (KeyValuePair<Type, string> configsResourcesPath in _configsResourcesPath)
            {
                ScriptableObject config = _resources.Load<ScriptableObject>(configsResourcesPath.Value);
                loadedConfigs.Add(configsResourcesPath.Key, config);

                yield return null;
            }

            onConfigsLoaded?.Invoke(loadedConfigs);
        }
    }
}
