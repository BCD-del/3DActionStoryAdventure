using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.LevelUPFeature;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay;
using Assets._Project.Develop.Runtime.UI.Gameplay.Stages;
using Assets._Project.Develop.Runtime.UI.Wallet;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenPresenter : IPresenter
    {
        private readonly GameplayScreenView _screen;

        private readonly GameplayPresentersFactory _gameplayPresentersFactory;
        private readonly ProjectPresentersFactory _projectPresentersFactory;

        private readonly List<IPresenter> _childPresenters = new();

        private EntitiesHealthDisplayPresenter _entitiesHealthDisplayPresenter;

        private readonly MainHeroHolderService _mainHeroHolderService;
        private IDisposable _mainHeroHolderServiceDisposable;
        private CurrencyPresenter _mainHeroCoinsPresenter;

        public GameplayScreenPresenter(
            GameplayScreenView screen,
            GameplayPresentersFactory gameplayPresentersFactory,
            MainHeroHolderService mainHeroHolderService,
            ProjectPresentersFactory projectPresentersFactory)
        {
            _screen = screen;
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _mainHeroHolderService = mainHeroHolderService;
            _projectPresentersFactory = projectPresentersFactory;
        }

        public void Initialize()
        {
            CreateStageNumber();
            CreateEntitiesHealthDisplay();
            CreateMainHeroExperienceView();

            _mainHeroHolderServiceDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnHeroRegistred);

            foreach (IPresenter presenter in _childPresenters)
                presenter.Initialize();
        }

        public void Dispose()
        {
            _mainHeroHolderServiceDisposable.Dispose();
            _mainHeroCoinsPresenter?.Dispose();

            foreach (IPresenter presenter in _childPresenters)
                presenter.Dispose();

            _childPresenters.Clear();
        }

        public void LateUpdate()
        {
            _entitiesHealthDisplayPresenter.LateUpdate();
        }

        private void OnHeroRegistred(Entity entity)
        {
            _mainHeroCoinsPresenter = _projectPresentersFactory.CreateCurrencyPresenter(_screen.CoinsView, entity.Coins, Meta.Features.Wallet.CurrencyTypes.Gold);

            _mainHeroCoinsPresenter.Initialize();
        }

        private void CreateStageNumber()
        {
            StagePresenter stagePresenter = _gameplayPresentersFactory.CreateStagePresenter(_screen.StageNumberView);

            _childPresenters.Add(stagePresenter);
        }

        private void CreateEntitiesHealthDisplay()
        {
            _entitiesHealthDisplayPresenter = _gameplayPresentersFactory.CreateEntitiesHealthDisplayPresenter(_screen.EntitiesHealthDisplay);

            _childPresenters.Add(_entitiesHealthDisplayPresenter);
        }

        private void CreateMainHeroExperienceView()
        {
            MainHeroExperiencePresenter experiencePresenter = _gameplayPresentersFactory.CreateMainHeroExperiencePresenter(_screen.ExpBarView);

            _childPresenters.Add(experiencePresenter);
        }
    }
}