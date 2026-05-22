using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay.New
{
    public class MainHeroHealthBarPresenter : IPresenter
    {
        private BarWithText _healthBarView;

        private ReactiveVariable<float> _currentHealth;
        private ReactiveVariable<float> _maxHealth;

        private List<IDisposable> _disposables = new();

        public MainHeroHealthBarPresenter(BarWithText healthBarView, ReactiveVariable<float> currentHealth, ReactiveVariable<float> maxHealth)
        {
            _healthBarView = healthBarView;
            _currentHealth = currentHealth;
            _maxHealth = maxHealth;
        }

        public void Initialize()
        {
            _disposables.Add(_currentHealth.Subscribe(OnCurrentHealthChanged));
            _disposables.Add(_maxHealth.Subscribe(OnMaxHealthChanged));
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
                disposable.Dispose();
        }

        private void OnCurrentHealthChanged(float oldValue, float newValue)
        {
            _healthBarView.UpdateSlider(_currentHealth.Value / _maxHealth.Value);
            _healthBarView.UpdateText($"{_currentHealth.Value}/{_maxHealth.Value}");
        }

        private void OnMaxHealthChanged(float oldValue, float newValue)
        {
            _healthBarView.UpdateSlider(_currentHealth.Value / _maxHealth.Value);
            _healthBarView.UpdateText($"{_currentHealth.Value}/{_maxHealth.Value}");
        }
    }
}
