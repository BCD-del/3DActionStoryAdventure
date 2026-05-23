using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay.New;
using System;

namespace Assets._Project.Develop.Runtime.UI.Gameplay
{
    public class GameplayScreenPresenter : IPresenter
    {
        private readonly GameplayScreenView _screenView;

        private readonly GameplayPresentersFactory _gameplayPresentersFactory;

        private readonly MainHeroHolderService _mainHeroHolderService;
        private IDisposable _mainHeroHolderServiceDisposable;

        public GameplayScreenPresenter(
            GameplayScreenView screenView,
            GameplayPresentersFactory gameplayPresentersFactory,
            MainHeroHolderService mainHeroHolderService)
        {
            _screenView = screenView;
            _gameplayPresentersFactory = gameplayPresentersFactory;
            _mainHeroHolderService = mainHeroHolderService;
        }

        public void Initialize()
        {
            _mainHeroHolderServiceDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnHeroRegistred);
        }

        public void Dispose()
        {
            _mainHeroHolderServiceDisposable.Dispose();
        }

        private void OnHeroRegistred(Entity entity)
        {
            MainHeroHealthBarPresenter mainHeroHealthPresenter = _gameplayPresentersFactory
                .CreateHeroHealthBarPresenter(
                _screenView.HealthBarView, 
                entity.CurrentHealth, 
                entity.MaxHealth);

            mainHeroHealthPresenter.Initialize();
        }
    }
}