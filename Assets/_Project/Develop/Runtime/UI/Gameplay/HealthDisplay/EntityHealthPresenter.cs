using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.TeamsFeature;
using Assets._Project.Develop.Runtime.UI.CommonViews;
using Assets._Project.Develop.Runtime.UI.Core;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.UI.Gameplay.HealthDisplay
{

    public class EntityHealthPresenter : IPresenter
    {
        private BarWithText _bar;

        private Entity _entity;
        private ReactiveVariable<Teams> _team; 
        private ReactiveVariable<float> _health; 
        private ReactiveVariable<float> _maxHealth;

        private List<IDisposable> _disposables = new();

        public EntityHealthPresenter(BarWithText bar, Entity entity)
        {
            _bar = bar;
            _entity = entity;
        }

        public BarWithText Bar => _bar;

        public void Initialize()
        {
            _health = _entity.CurrentHealth;
            _maxHealth = _entity.MaxHealth;
            _team = _entity.Team;

            _disposables.Add(_health.Subscribe(OnHealthChanged));
            _disposables.Add(_maxHealth.Subscribe(OnMaxHealthChanged));
            _disposables.Add(_team.Subscribe(OnTeamChanged));

            UpdateHealth();
            UpdateFillerColorBy(_team.Value);
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();
        }

        private void OnHealthChanged(float arg1, float newValue)
        {
            UpdateHealth();
        }

        private void OnMaxHealthChanged(float arg1, float newValue)
        {
            UpdateHealth();
        }

        private void OnTeamChanged(Teams teams1, Teams newTeam)
        {
            UpdateFillerColorBy(newTeam);
        }

        private void UpdateHealth()
        {
            _bar.UpdateText(_health.Value.ToString("0"));
            _bar.UpdateSlider(_health.Value / _maxHealth.Value);
        }

        private void UpdateFillerColorBy(Teams value)
        {
            switch (value)
            {
                case Teams.MainHero:
                    _bar.SetFillerColor(Color.green);
                    break;

                case Teams.Enemies:
                    _bar.SetFillerColor(Color.red);
                    break;
            }
        }
    }
}