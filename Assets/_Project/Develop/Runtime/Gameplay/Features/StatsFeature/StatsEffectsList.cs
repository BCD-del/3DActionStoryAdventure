using System.Collections.Generic;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature
{
    public class StatsEffectsList
    {
        public event Action<IStatsEffect> Added;
        public event Action<IStatsEffect> Removed;

        private List<IStatsEffect> _elements = new();

        public IReadOnlyList<IStatsEffect> Elements => _elements;

        public virtual void Add(IStatsEffect element)
        {
            _elements.Add(element);
            Added?.Invoke(element);
        }

        public virtual void Remove(IStatsEffect element)
        {
            _elements.Remove(element);
            Removed?.Invoke(element);
        }
    }
}
