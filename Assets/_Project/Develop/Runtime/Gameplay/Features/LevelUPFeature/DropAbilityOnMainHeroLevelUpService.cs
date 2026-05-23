using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.MainHero;
using Assets._Project.Develop.Runtime.UI.Gameplay;
using Assets._Project.Develop.Runtime.Utilites.CoroutinesManagment;
using Assets.CourseGame.Develop.Gameplay.Features.AbilitiesFeature.View;
using System.Collections.Generic;
using System;
using UnityEngine;
using Assets._Project.Develop.Runtime.Infrastructure.DI;
using System.Collections;
using Assets._Project.Develop.Runtime.Gameplay.Features.PauseFeature;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LevelUPFeature
{
    public class DropAbilityOnMainHeroLevelUpService : IInitializable, IDisposable
    {
        private MainHeroHolderService _mainHeroHolderService;
        private GameplayPopupService _popupService;
        private ICoroutinesPerformer _coroutinePerformer;
        private IPauseService _pauseService;

        private Queue<int> _levelUpRequests = new();

        private AbilitySelectPopupPresenter _popup;
        private Coroutine _selectAbilityProcess;

        private IDisposable _heroRegistredDisposable;
        private IDisposable _heroLevelChangedDisposable;

        public DropAbilityOnMainHeroLevelUpService(
            MainHeroHolderService mainHeroHolderService,
            GameplayPopupService popupService,
            ICoroutinesPerformer coroutinePerformer,
            IPauseService pauseService)
        {
            _mainHeroHolderService = mainHeroHolderService;
            _popupService = popupService;
            _coroutinePerformer = coroutinePerformer;
            _pauseService = pauseService;
        }

        private bool PopupIsOpened => _popup != null;

        public void Initialize()
        {
           // _heroRegistredDisposable = _mainHeroHolderService.HeroRegistred.Subscribe(OnMainHeroRegistred);
        }

        public void Dispose()
        {
            _heroRegistredDisposable.Dispose();
            _heroLevelChangedDisposable?.Dispose();
        }

        private void OnMainHeroRegistred(Entity hero)
        {
            _heroLevelChangedDisposable = hero.Level.Subscribe(OnHeroLevelChanged);
        }

        private void OnHeroLevelChanged(int arg1, int currentLevel)
        {
            _levelUpRequests.Enqueue(currentLevel);

            if (_selectAbilityProcess != null)
                return;

            _selectAbilityProcess = _coroutinePerformer.StartPerform(SelectAbilityProcess());
        }

        private IEnumerator SelectAbilityProcess()
        {
            while (_levelUpRequests.Count > 0)
            {
                int level = _levelUpRequests.Dequeue();

                _pauseService.Pause();
                _popup = _popupService.OpenAbilitySelectPopup(_mainHeroHolderService.MainHero, level, () =>
                {
                    _pauseService.Unpause();
                    _popup = null;
                });

                yield return new WaitUntil(() => PopupIsOpened == false);
            }

            _selectAbilityProcess = null;
        }
    }
}