using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Conditions;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    public class CharacterControllerLocomotionSystem : IInitializableSystem, IUpdatableSystem
    {
        private CharacterController _characterController;

        private ReactiveVariable<Vector3> _moveDirection;
        private ReactiveVariable<Vector3> _rotationDirection;

        private ReactiveVariable<float> _moveSpeed;
        private ReactiveVariable<float> _rotationSpeed;

        private ReactiveVariable<bool> _isMoving;

        private ICompositeCondition _canMove;

        public void OnInit(Entity entity)
        {
            _moveDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;

            _moveSpeed = entity.MoveSpeed;
            _rotationSpeed = entity.RotationSpeed;

            _characterController = entity.CharacterController;

            _isMoving = entity.IsMoving;

            _canMove = entity.CanMove;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_canMove.Evaluate() == false)
            {
                _isMoving.Value = false;
                return;
            }

            Vector3 moveDir = _moveDirection.Value;
            Vector3 rotationDir = _rotationDirection.Value;
            Vector3 velocity = moveDir.normalized * _moveSpeed.Value;

            _characterController.Move(velocity * deltaTime);

            _isMoving.Value = velocity.sqrMagnitude > 0.001f;

            if (rotationDir.sqrMagnitude > 0.001f)
            {
                rotationDir.y = 0f;


                Quaternion targetRotation = Quaternion.LookRotation(rotationDir);
                Transform charTransform = _characterController.transform;

                charTransform.rotation = Quaternion.Slerp(
                    charTransform.rotation,
                    targetRotation,
                    _rotationSpeed.Value * deltaTime
                );

            }
        }
    }
}
