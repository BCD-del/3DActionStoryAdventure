using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Conditions;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using System.Collections.Generic;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.LifeCycle
{

    public class HealthBarPoint : IEntityComponent
    {
        public Transform Value;
    }
        
    public class CurrentHealth : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class MaxHealth : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class MustDie : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class MustSelfRelease : IEntityComponent
    {
        public ICompositeCondition Value;
    }

    public class IsDead : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class DeathProcessInitialTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class DeathProcessCurrentTime : IEntityComponent
    {
        public ReactiveVariable<float> Value;
    }

    public class InDeathProcess : IEntityComponent
    {
        public ReactiveVariable<bool> Value;
    }

    public class DisableCollidersOnDeath : IEntityComponent
    {
        public List<Collider> Value;
    }
}
