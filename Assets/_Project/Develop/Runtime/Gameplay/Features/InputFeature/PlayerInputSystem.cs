using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore;
using Assets._Project.Develop.Runtime.Gameplay.EntitiesCore.Systems;
using Assets._Project.Develop.Runtime.Gameplay.Features.Camera;
using Assets._Project.Develop.Runtime.Utilites.Reactive;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class PlayerInputSystem : IInitializableSystem, IUpdatableSystem
    {
        private readonly IInputService _input;
        private readonly CameraService _cameraService;

        private ReactiveVariable<Vector3> _moveDirection;
        private ReactiveVariable<Vector3> _rotationDirection;

        public PlayerInputSystem(IInputService input, CameraService cameraService)
        {
            _input = input;
            _cameraService = cameraService;
        }

        public void OnInit(Entity entity)
        {
            _moveDirection = entity.MoveDirection;
            _rotationDirection = entity.RotationDirection;
        }

        public void OnUpdate(float deltaTime)
        {
            Vector3 rawInput = _input.MoveDirection;
            Vector2 moveInput = new Vector2(rawInput.x, rawInput.z);

            UpdateOrbitMovement(moveInput);

            Vector2 lookInput = _input.LookDirection;
            float zoom = _input.ZoomInput;

            _cameraService.Update(deltaTime, lookInput, zoom);
        }

        private void UpdateOrbitMovement(Vector2 moveInput)
        {
            Vector3 direction = CalculateCameraRelativeDirection(moveInput);
            _moveDirection.Value = direction;
            _rotationDirection.Value = direction;
        }

        private Vector3 CalculateCameraRelativeDirection(Vector2 input)
        {
            Transform cameraTransform = _cameraService.CameraTransform;

            if (cameraTransform == null)
                return new Vector3(input.x, 0f, input.y);

            Vector3 forward = cameraTransform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = cameraTransform.right; right.y = 0f; right.Normalize();

            return forward * input.y + right * input.x;
        }
    }
}