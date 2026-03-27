using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature
{
    public class MaxHealthStatSynchronizerSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<float> _maxHealth;
        private ReactiveVariable<float> _currentHealth;
        private Dictionary<StatTypes, float> _modifiedStats;

        public void OnInit(Entity entity)
        {
            _maxHealth = entity.MaxHealth;
            _currentHealth = entity.CurrentHealth;
            _modifiedStats = entity.ModifiedStats;
        }

        public void OnUpdate(float deltaTime)
        {
            float tempValue = _modifiedStats[StatTypes.MaxHealth];

            float previousRatio = _currentHealth.Value / _maxHealth.Value;

            if (tempValue < 0)
                tempValue = 0;

            _maxHealth.Value = tempValue;
            _currentHealth.Value = _maxHealth.Value * previousRatio;
        }
    }
}