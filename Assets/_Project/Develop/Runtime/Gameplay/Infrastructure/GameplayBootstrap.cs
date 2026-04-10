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

namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayBootstrap : SceneBootstrap
    {
        private DIContainer _container;
        private GameplayInputArgs _inputArgs;

        private EntitiesLifeContext _entitiesLifeContext;

        private GameObject _mainHero;

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

            ConfigsProviderService configsProviderService = _container.Resolve<ConfigsProviderService>();
            MainHeroConfig config = configsProviderService.GetConfig<MainHeroConfig>();
            MainHeroFactory mainHeroFactory = _container.Resolve<MainHeroFactory>();

            GameObject hero = mainHeroFactory.CreateMainHero(config, Vector3.up * 10);


            yield break;
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