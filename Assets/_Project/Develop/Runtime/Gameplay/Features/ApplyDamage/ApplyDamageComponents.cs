using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Conditions;
using Assets._Project.Develop.Runtime.Utilites.Reactive;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.ApplyDamage
{
    public class TakeDamageRequest : IEntityComponent
    {
        public ReactiveEvent<float> Value;
    }

    public class TakeDamageEvent : IEntityComponent
    {
        public ReactiveEvent<float> Value;
    }

    public class CanApplyDamage : IEntityComponent
    {
        public ICompositeCondition Value;
    }
}
