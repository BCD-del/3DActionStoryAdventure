using Assets._Project.Develop.Runtime.Configs;
using Assets._Project.Develop.Runtime.InputFeature;
using Assets._Project.Develop.Runtime.MovementFeatures;
using Assets._Project.Develop.Runtime.PhysicsFeatures;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Transform _legs;
        [SerializeField] private PlayerConfig _config;

        private IPlayerInput _input;
        private RigidbodyDirectionalMover _mover;
        private RigidbodyDirectionalRotator _rotator;
        private RigidbodyJumpHandler _jumpHandler;
        private GroundChecker _groundChecker;
        private GravityHandler _gravityHandler;

        public bool IsRunning => _rigidbody.linearVelocity.magnitude > 0;

        private void Start()
        {
            _input = new PCPlayerInput();
            _mover = new RigidbodyDirectionalMover(_rigidbody, _config.MovementSpeed);
            _rotator = new RigidbodyDirectionalRotator(_rigidbody, _config.RotationSpeed);
            _jumpHandler = new RigidbodyJumpHandler(_rigidbody, _config.JumpPower);
            _groundChecker = new GroundChecker(_legs, _config.LegsRange, _config.JumpableMask);
            _gravityHandler = new GravityHandler(_config.Gravity, _rigidbody, _groundChecker);
        }

        private void Update()
        {
            Vector3 inputDirection = _input.GetMovementDirection();

            _mover.Move(inputDirection);
            _rotator.Rotate(inputDirection, Time.deltaTime);

            _gravityHandler.ApplyGravity(Time.deltaTime);

            if (_input.IsJumpKeyPressed() && _groundChecker.IsTouched())
            {
                _jumpHandler.Jump();
            }      
        }
    }
}
