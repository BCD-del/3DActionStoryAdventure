using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Conditions;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    public class RigidbodyMovementSystem : IInitializableSystem, IUpdatableSystem
    {
        private ReactiveVariable<Vector3> _moveDirection;
        private ReactiveVariable<float> _moveSpeed;
        private Rigidbody _rigidbody;
        private ReactiveVariable<bool> _isMoving;

        private ICompositeCondition _canMove;

        public void OnInit(Entity entity)
        {
            _moveDirection = entity.MoveDirection;
            _moveSpeed = entity.MoveSpeed;
            _rigidbody = entity.Rigidbody;
            _canMove = entity.CanMove;
            _isMoving = entity.IsMoving;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_canMove.Evaluate() == false)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                return;
            }

            Vector3 velocity = _moveDirection.Value.normalized * _moveSpeed.Value;

            _isMoving.Value = _rigidbody.linearVelocity.magnitude > 0;

            _rigidbody.linearVelocity = velocity;
        }
    }
}
