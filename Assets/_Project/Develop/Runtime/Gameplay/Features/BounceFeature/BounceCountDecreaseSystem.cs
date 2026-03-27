using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.BounceFeature
{
    public class BounceCountDecreaseSystem : IInitializableSystem, IDisposableSystem
    {
        private ReactiveVariable<int> _bounceCount;
        private ReactiveEvent<RaycastHit> _bounceEvent;

        private IDisposable _bounceDisposable;

        public void OnInit(Entity entity)
        {
            _bounceCount = entity.BounceCount;
            _bounceEvent = entity.BounceEvent;

            _bounceDisposable = _bounceEvent.Subscribe(OnBounceEvent);
        }

        private void OnBounceEvent(RaycastHit hit)
        {
            _bounceCount.Value--;
        }

        public void OnDispose()
        {
            _bounceDisposable.Dispose();
        }
    }
}