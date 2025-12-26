using Assets._Project.Develop.Runtime.InputFeature;
using Assets._Project.Develop.Runtime.MovementFeatures;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _movementSpeed = 5;
        [SerializeField] private float _rotationSpeed = 90;

        private IPlayerInput _input;
        private RigidbodyDirectionalMover _mover;
        private RigidbodyDirectionalRotator _rotator;

        private void Start()
        {
            _input = new PCPlayerInput();
            _mover = new RigidbodyDirectionalMover(_rigidbody, _movementSpeed);
            _rotator = new RigidbodyDirectionalRotator(_rigidbody, _rotationSpeed);
        }

        private void Update()
        {
            Vector3 inputDirection = _input.GetMovementDirection();

            _mover.Move(inputDirection);
            _rotator.Rotate(inputDirection, Time.deltaTime);
        }
    }
}
