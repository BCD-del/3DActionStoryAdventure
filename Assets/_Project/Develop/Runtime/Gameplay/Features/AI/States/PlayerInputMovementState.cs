using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using Assets._Project.Develop.Runtime.Utilites.StateMachineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.AI.States
{
    public class PlayerInputMovementState : State, IUpdatableState
    {
        private IInputService _inputService;

        private ReactiveVariable<Vector3> _movementDirection;
        private ReactiveVariable<Vector3> _rotationDirection;

        public PlayerInputMovementState(Entity entity, IInputService inputService)
        {
            _inputService = inputService;

            _movementDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
        }

        public void Update(float deltaTime)
        {
            _movementDirection.Value = _inputService.MoveDirection;
            _rotationDirection.Value = _inputService.MoveDirection;
        }

        public override void Exit()
        {
            base.Exit();

            _movementDirection.Value = Vector3.zero;
        }
    }
}
