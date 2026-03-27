using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class RotateToTargetState : State, IUpdatableState
    {
        private ReactiveVariable<Vector3> _rotationDirection; 
        private ReactiveVariable<Entity> _currentTarget;
        private Transform _transform;

        public RotateToTargetState(Entity entity)
        {
            _rotationDirection = entity.RotationDirection;
            _currentTarget = entity.CurrentTarget;
            _transform = entity.Transform;
        }

        public void Update(float deltaTime)
        {
            if (_currentTarget.Value != null)
            {
                _rotationDirection.Value = (_currentTarget.Value.Transform.position - _transform.position).normalized;
            }
        }
    }
}
