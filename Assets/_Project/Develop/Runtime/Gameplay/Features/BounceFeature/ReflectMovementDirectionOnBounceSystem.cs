using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;
using System;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.BounceFeature
{
    public class ReflectMovementDirectionOnBounceSystem : IInitializableSystem, IDisposableSystem
    {
        private Transform _transform;
        private ReactiveVariable<Vector3> _movementDirection;
        private ReactiveEvent<RaycastHit> _bounceEvent;

        private IDisposable _bounceDisposable;

        public void OnInit(Entity entity)
        {
            _transform = entity.Transform;
            _movementDirection = entity.MoveDirection;
            _bounceEvent = entity.BounceEvent;

            _bounceDisposable = _bounceEvent.Subscribe(OnBounceEvent);
        }

        private void OnBounceEvent(RaycastHit hit)
        {
            _movementDirection.Value = Vector3.Reflect(_movementDirection.Value, hit.normal);
            _transform.position = hit.point + hit.normal * 0.1f;
        }

        public void OnDispose()
        {
            _bounceDisposable.Dispose();
        }
    }
}
