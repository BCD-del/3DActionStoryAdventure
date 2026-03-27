using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.BounceFeature
{
    public class LayerToBounceReaction : IEntityComponent
    {
        public LayerMask Value;
    }

    public class BounceEvent : IEntityComponent
    {
        public ReactiveEvent<RaycastHit> Value;
    }

    public class BounceCount : IEntityComponent
    {
        public ReactiveVariable<int> Value;
    }
}