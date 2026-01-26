using Assets._Project.Develop.Runtime.Configs;
using Assets._Project.Develop.Runtime.InputFeature;
using Assets._Project.Develop.Runtime.Рефакторинг.Components;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Рефакторинг
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _legsPoint;
        [SerializeField] private PlayerConfig _config;
        [SerializeField] private Camera _camera;
        [SerializeField] private LayerMask _groundMask;
        [SerializeField] private GameObject _targetOFTracking;

        private PCPlayerInput _input;
        private PlayerMoverManager _moverManager;
        private PlayerLookDirection _lookDirection;
        private PlayerJump _jump;
        public bool OnGround => Physics.OverlapSphere(_legsPoint.position, 0.5f, _groundMask).Length > 0;

        public void Start()
        {
            _input = new PCPlayerInput();

            _moverManager = GetComponent<PlayerMoverManager>();
            _lookDirection = new PlayerLookDirection(_camera, _config.Sensivity, _config.VerticalLimit, _targetOFTracking);
            _jump = new PlayerJump(_config.JumpPower, _legsPoint, _config.JumpableMask, _rigidbody);
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            _moverManager.SetMoveInput(transform.right * _input.Horizontal + transform.forward * _input.Vertical);
            _lookDirection.Look(_input.MouseX, _input.MouseY);
            _jump.TryJump(_input.Jump);
        }
    }
}