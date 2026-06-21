using UnityEngine;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.InputFeature
{
    public class DesktopInput : IInputService
    {
        private readonly PlayerInput _playerInput;

        public DesktopInput(PlayerInput playerInput)
        {
            _playerInput = playerInput;

            _playerInput.Enable();
        }

        public Vector3 MoveDirection
        {
            get
            {
                Vector2 v = _playerInput.Desktop.Move.ReadValue<Vector2>();
                return new Vector3(v.x, 0f, v.y);
            }
        }

        public Vector2 LookDirection => _playerInput.Desktop.Look.ReadValue<Vector2>();

        public float ZoomInput => _playerInput.Desktop.Zoom.ReadValue<float>();

        public bool IsEnabled
        {
            get => true;

            set
            {
                if (value)
                    _playerInput.Enable();
                else
                    _playerInput.Disable();
            }
        }

        public bool IsAttackKeyPressed => _playerInput.Desktop.Attack.IsPressed();     
    }
}