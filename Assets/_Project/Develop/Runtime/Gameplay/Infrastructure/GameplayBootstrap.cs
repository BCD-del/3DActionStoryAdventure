using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Infrastructure;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using System;
using System.Collections;
using UnityEngine;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Utilites.ConfigsManagment;
using Assets._Project.Develop.Runtime.Configs.MainHero;
using Assets._Project.Develop.NPCConfigsGameplay.NPCConfigs;
using Assets._Project.Develop.Runtime.Gameplay.Features.NPC;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Levels;

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayBootstrap : SceneBootstrap
    {
        private DIContainer _container;
        private GameplayInputArgs _inputArgs;

        private EntitiesLifeContext _entitiesLifeContext;

        private GameObject _mainHero;

        private GameObject _npc;

        private AIBrainsContext _brainsContext;

        public override void ProcessRegistrations(DIContainer container, IInputSceneArgs sceneArgs = null)
        {
            _container = container;

            if (sceneArgs is not GameplayInputArgs gameplayInputArgs)
                throw new ArgumentException($"{nameof(sceneArgs)} is not match with {typeof(GameplayInputArgs)} type");

            _inputArgs = gameplayInputArgs;

            GameplayContextRegistrations.Process(_container, _inputArgs);
        }

        public override IEnumerator Initialize()
        {
            Debug.Log($"Вы попали на уровень {_inputArgs.LevelNumber}");

            Debug.Log("Инициализация геймплейной сцены");

            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();
            _brainsContext = _container.Resolve<AIBrainsContext>();

            CreateMainHero();
            CreateNPC();
            yield break;
        }

        private void CreateNPC()
        {
            ConfigsProviderService configsProviderService = _container.Resolve<ConfigsProviderService>();
            NPCConfig config = configsProviderService.GetConfig<NPCConfig>();
            GameObject npc = _container.Resolve<NPCFactory>().CreateNPC(config);
            npc.transform.position = configsProviderService.GetConfig<LevelsListConfig>().GetBy(_inputArgs.LevelNumber).NPCSpawnPoint;
        }

        private void CreateMainHero()
        {
            ConfigsProviderService configsProviderService = _container.Resolve<ConfigsProviderService>();
            MainHeroConfig config = configsProviderService.GetConfig<MainHeroConfig>();
            MainHeroFactory mainHeroFactory = _container.Resolve<MainHeroFactory>();

            _mainHero = mainHeroFactory.CreateMainHero(config, configsProviderService.GetConfig<LevelsListConfig>().GetBy(_inputArgs.LevelNumber).MainHeroSpawnPoint);
        }

        public override void Run()
        {
            Debug.Log("Старт геймплейной сцены");
        }

        private void Update()
        {
            _brainsContext?.Update(Time.deltaTime);
            _entitiesLifeContext?.Update(Time.deltaTime);
        }
    }
}