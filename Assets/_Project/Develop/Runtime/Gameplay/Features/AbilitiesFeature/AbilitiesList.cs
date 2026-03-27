using Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature;
using System.Collections.Generic;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AbilitiesFeature
{
    public class AbilitiesList
    {
        public event Action<Ability> Added;

        private List<Ability> _elements = new();

        public IReadOnlyList<Ability> Elements => _elements;

        public virtual void Add(Ability element)
        {
            _elements.Add(element);
            Added?.Invoke(element);
        }
    }
}
