using System;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature
{
    public class StatsEffect : IStatsEffect
    {
        private StatTypes _statType;
        private Func<float, float> _applyEffect;

        public StatsEffect(StatTypes statType, Func<float, float> applyEffect)
        {
            _statType = statType;
            _applyEffect = applyEffect;
        }

        public void ApplyTo(Dictionary<StatTypes, float> stats)
        {
            stats[_statType] = _applyEffect(stats[_statType]);
        }
    }
}
