using Assets._Project.Develop.Runtime.Configs.Gameplay;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LevelUPFeature
{
    public class MainHeroExperiencePresenter : IPresenter
    {
        private BarWithText _view;

        private MainHeroHolderService _mainHeroHolderService;
        private ExperienceForUpgradeLevelConfig _levelUpConfig;
        private ReactiveVariable<float> _experience;
        private ReactiveVariable<int> _currentLevel;

        private List<IDisposable> _disposables = new();

        public MainHeroExperiencePresenter(
            MainHeroHolderService mainHeroHolderService,
            BarWithText view,
            ExperienceForUpgradeLevelConfig levelUpConfig)
        {
            _mainHeroHolderService = mainHeroHolderService;
            _view = view;
            _levelUpConfig = levelUpConfig;
        }

        public void Initialize()
        {
            // _disposables.Add(_mainHeroHolderService.HeroRegistred.Subscribe(OnMainHeroRegistred));
        }

        private void OnMainHeroRegistred(Entity entity)
        {
            _experience = entity.Experience;
            _currentLevel = entity.Level;

            _disposables.Add(_experience.Subscribe(OnCurrentExperienceChanged));
            _disposables.Add(_currentLevel.Subscribe(OnLevelChanged));

            UpdateBarText(_currentLevel.Value);
            UpdateCurrentExperience(_experience.Value);
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();
        }

        private void UpdateCurrentExperience(float value)
            => _view.UpdateSlider(value / _levelUpConfig.GetExperienceFor(_currentLevel.Value));

        private void UpdateBarText(int level) => _view.UpdateText($"LvL.{level}");

        private void OnLevelChanged(int arg1, int arg2)
            => UpdateBarText(_currentLevel.Value);

        private void OnCurrentExperienceChanged(float arg1, float arg2)
            => UpdateCurrentExperience(_experience.Value);
    }
}