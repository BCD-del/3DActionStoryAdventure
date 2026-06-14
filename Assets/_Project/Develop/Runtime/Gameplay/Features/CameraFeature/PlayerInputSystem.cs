using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;


namespace Assets._Project.Develop.Runtime.Gameplay.Features.CameraFeature
{
    public class PlayerInputSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IInputService _inputService;

        private ReactiveVariable<Vector3> _moveDirection;
        private ReactiveEvent _startAttackRequest;


        public PlayerInputSystem(IInputService inputService)
        {
            _inputService = inputService;
        }

        public void OnInit(Entity entity)
        {
            _moveDirection = entity.MoveDirection;
            _startAttackRequest = entity.StartAttackRequest;
        }

        public void OnUpdate(float deltaTime)
        {
            _moveDirection.Value = _inputService.MoveDirection;

            if (_inputService.IsAttackKeyPressed)
                _startAttackRequest?.Invoke();
        }
    }
}
