using Assets._Project.Develop.Runtime.Gameplay.Features.StageFeature;
using Assets._Project.Develop.Runtime.UI.Core;
using System;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.Stages
{
    public class StagePresenter : IPresenter
    {
        private readonly IconTextView _view;
        private readonly StageProviderService _stageProviderService;

        private IDisposable _currentStageNumberChangedDisposable;

        public StagePresenter(IconTextView view, StageProviderService stageProviderService)
        {
            _view = view;
            _stageProviderService = stageProviderService;
        }

        public void Initialize()
        {
            _currentStageNumberChangedDisposable = _stageProviderService.CurrentStageNumber.Subscribe(OnNextStageIndexChanged);

            UpdateStageNumber();
        }

        private void UpdateStageNumber()
        {
            _view.SetText($"{_stageProviderService.CurrentStageNumber.Value} / {_stageProviderService.StagesCount}");
        }

        public void Dispose()
        {
            _currentStageNumberChangedDisposable.Dispose(); 
        }

        private void OnNextStageIndexChanged(int arg1, int arg2)
        {
            UpdateStageNumber();
        }
    }
}