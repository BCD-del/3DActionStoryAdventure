using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.MovementFeature
{
    public class CharacterControllerLocomotionSystem : IInitializableSystem, IUpdatableSystem
    {
        private CharacterController _controller;

        private ReactiveVariable<float> _moveSpeed;
        private ReactiveVariable<Vector3> _moveDirection;

        public void OnInit(Entity entity)
        {
            _controller = entity.CharacterController;
            _moveSpeed = entity.MoveSpeed;
            _moveDirection = entity.MoveDirection;
        }

        public void OnUpdate(float deltaTime)
        {
            _controller.Move(_moveDirection.Value * _moveSpeed.Value * Time.deltaTime);
        }
    }
}
