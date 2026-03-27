using Assets._Project.Develop.Runtime.Configs.Gameplay;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Entities;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI.States;
using Assets._Project.Develop.Runtime.Gameplay.Features.LevelUPFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Utilites.ConfigsManagment;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using Assets._Project.Develop.Runtime.Meta.Features.StatsUpgrade;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MainHero
{
    public class MainHeroFactory
    {
        private readonly DIContainer _container;

        private readonly EntitiesFactory _entitiesFactory;
        private readonly BrainsFactory _brainsFactory;
        private readonly ConfigsProviderService _configsProviderService;
        private readonly EntitiesLifeContext _entitiesLifeContext;
        private readonly StatsUpgradeService _statsUpgradeService;

        public MainHeroFactory(DIContainer container)
        {
            _container = container;
            _entitiesFactory = _container.Resolve<EntitiesFactory>();
            _brainsFactory = _container.Resolve<BrainsFactory>();
            _configsProviderService = _container.Resolve<ConfigsProviderService>();
            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _statsUpgradeService = _container.Resolve<StatsUpgradeService>();
        }

        public Entity Create(Vector3 position)
        {
            HeroConfig config = _configsProviderService.GetConfig<HeroConfig>();

            Entity entity = _entitiesFactory.CreateHero(position, config, GetStats());

            entity
                .AddIsMainHero()
                .AddTeam(new ReactiveVariable<Teams>(Teams.MainHero));

            entity
                .AddAbilities()
                .AddSystem(new AbilityOnAddActivatorSystem());

            entity
                .AddCoins();

            entity
                .AddLevel(new ReactiveVariable<int>(1))
                .AddExperience()
                .AddSystem(new LevelUpSystem(_configsProviderService.GetConfig<ExperienceForUpgradeLevelConfig>()));

            entity.AddCurrentTarget();
            _brainsFactory.CreateMainHeroBrain(entity, new NearestDamagableTargetSelector(entity));

            _entitiesLifeContext.Add(entity);

            return entity;
        }


        private Dictionary<StatTypes, float> GetStats()
        {
            Dictionary<StatTypes, float> stats = new();
            foreach (StatTypes statType in _statsUpgradeService.AvailableStats)
            {
                if (_statsUpgradeService.IsStatUpgradeable(statType))
                    stats.Add(statType, _statsUpgradeService.GetCurrentStatValueFor(statType));
            }
            return stats;
        }
    }
}