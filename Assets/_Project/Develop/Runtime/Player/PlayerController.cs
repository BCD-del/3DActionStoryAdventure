using Assets._Project.Develop.Runtime.InputFeature;
using Assets._Project.Develop.Runtime.MovementFeatures;
using Assets._Project.Develop.Runtime.PhysicsFeatures;
using UnityEngine;

namespace Assets._Project.Develop.Runtime.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private float _movementSpeed = 5;
        [SerializeField] private float _rotationSpeed = 90;
        [SerializeField] private float _jumpPower = 10;
        [SerializeField] private Transform _legs;
        [SerializeField] private float _legsRange = 0.25f;
        [SerializeField] private LayerMask _jumpableMask;
        [SerializeField] private float _gravity;

        private IPlayerInput _input;
        private RigidbodyDirectionalMover _mover;
        private RigidbodyDirectionalRotator _rotator;
        private RigidbodyJumpHandler _jumpHandler;
        private GroundChecker _groundChecker;
        private GravityHandler _gravityHandler;
        
        private void Start()
        {
            _input = new PCPlayerInput();
            _mover = new RigidbodyDirectionalMover(_rigidbody, _movementSpeed);
            _rotator = new RigidbodyDirectionalRotator(_rigidbody, _rotationSpeed);
            _jumpHandler = new RigidbodyJumpHandler(_rigidbody, _jumpPower);
            _jumpHandler = new RigidbodyJumpHandler(_rigidbody, _gravity);
            _groundChecker = new GroundChecker(_legs, _legsRange, _jumpableMask);

            // сделать класс GravityHandler и в нем прописать логику применения гравитации к некому _rigidbody объекту
            // логика такая: в этом классе (игрока) в апдейте вызывается метод ApplyGravity у обработчика гравитавции
            // метод ApplyGravity проверяет: если персонаж стоит на земле, то гравитация равна -2 
            // иначе гравитация постепенно уменьшается от -2 и применяется к _rigidbody.linearVelocity по направлению вниз
        }

        private void Update()
        {
            Vector3 inputDirection = _input.GetMovementDirection();

            _mover.Move(inputDirection);
            _rotator.Rotate(inputDirection, Time.deltaTime); 

            if (_input.IsJumpKeyPressed() && _groundChecker.IsTouched())
            {
                _jumpHandler.Jump();
            }
            if (_groundChecker.IsTouched() == false)
            {
                _gravityHandler.ApplyGravity();
            }
        }
    }  
}
