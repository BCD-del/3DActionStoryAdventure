using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System.Collections.Generic;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.StatsFeature
{
    public class BaseStats : IEntityComponent
    {
        public Dictionary<StatTypes, float> Value;
    }

    public class ModifiedStats : IEntityComponent
    {
        public Dictionary<StatTypes, float> Value;
    }

    public class StatsEffects : IEntityComponent
    {
        public StatsEffectsList Value;
    }

    public class AttacksPerSecond : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }
}
