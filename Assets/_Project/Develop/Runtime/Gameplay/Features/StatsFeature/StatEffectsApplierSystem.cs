using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature
{
    public class StatEffectsApplierSystem : IInitializableSystem, IDisposableSystem
    {
        private StatsEffectsList _statsEffects;
        private Dictionary<StatTypes, float> _baseStats;
        private Dictionary<StatTypes, float> _modifiedStats;

        public void OnInit(Entity entity)
        {
            _statsEffects = entity.StatsEffects;
            _baseStats = entity.BaseStats;
            _modifiedStats = entity.ModifiedStats;

            _statsEffects.Added += OnStatEffectAdded;
            _statsEffects.Removed += OnStatEffectRemoved;

            RecalculateStats();
        }

        public void OnDispose()
        {
            _statsEffects.Added -= OnStatEffectAdded;
            _statsEffects.Removed -= OnStatEffectRemoved;
        }

        private void OnStatEffectRemoved(IStatsEffect statEffect)
            => RecalculateStats();

        private void OnStatEffectAdded(IStatsEffect statEffect)
            => RecalculateStats();

        private void RecalculateStats()
        {
            foreach (StatTypes stat in _baseStats.Keys)
                _modifiedStats[stat] = _baseStats[stat];

            foreach (IStatsEffect statsEffect in _statsEffects.Elements)
                statsEffect.ApplyTo(_modifiedStats);
        }
    }
}

