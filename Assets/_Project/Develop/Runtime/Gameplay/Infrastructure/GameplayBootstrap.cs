using Assets._Project.Develop.Infrastructure;
using Assets._Project.Develop.Infrastructure.DI;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.AI;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.Gameplay.States;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilites.SceneManagement;
using System;
using System.Collections;
using UnityEngine;


namespace Assets._Project.Develop.Runtime.Gameplay.Infrastructure
{
    public class GameplayBootstrap : SceneBootstrap
    {
        private DIContainer _container;

        private GameplayInputArgs _inputArgs;

        private GameplayStatesContext _gameplayStatesContext;

        private GameplayScreenPresenter _screenPresenter;

        private EntitiesLifeContext _entitiesLifeContext;

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
            _screenPresenter = _container.Resolve<GameplayScreenPresenter>();

            _entitiesLifeContext = _container.Resolve<EntitiesLifeContext>();

            _brainsContext = _container.Resolve<AIBrainsContext>();

            _gameplayStatesContext = _container.Resolve<GameplayStatesContext>();

            _container.Resolve<MainHeroFactory>().Create(Vector3.zero);

            yield break;
        }

        public override void Run()
        {
            _gameplayStatesContext.Run();
        }

        private void Update()
        {
            _brainsContext?.Update(Time.deltaTime);

            _entitiesLifeContext?.Update(Time.deltaTime);

            _gameplayStatesContext?.Update(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.X))
            {
                _container.Resolve<MainHeroHolderService>().MainHero.Experience.Value += 50;
            }
        }

        private void LateUpdate()
        {
            _screenPresenter?.LateUpdate();
        }
    }
}
