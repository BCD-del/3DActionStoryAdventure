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

        public void OnInit(Entity entity)
        {
            _controller = entity.CharacterController;
            _moveSpeed = entity.MoveSpeed;
        }

        public void OnUpdate(float deltaTime)
        {
            if (Input.GetKey(KeyCode.W))
            {
                _controller.Move(Vector3.forward * _moveSpeed.Value * Time.deltaTime);
            }
        }
    }
}
